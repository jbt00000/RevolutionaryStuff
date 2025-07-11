﻿using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.ApiCore.Json;
using RevolutionaryStuff.ApiCore.Middleware;
using RevolutionaryStuff.ApiCore.OpenApi;
using RevolutionaryStuff.ApiCore.Services.ServerInfoFinders;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Services.ApplicationNameFinders;

namespace RevolutionaryStuff.ApiCore.Startup;

public abstract class ApiProgram
{
    private class Config
    {
        public const string ConfigSectionName = "ApiProgram";

        public bool ConfigureForRazorWebsite { get; set; }
        public bool EnableOpenApi { get; set; }
        public bool EnableIndexRoute { get; set; }
        public bool EnableServerInfoRoute { get; set; }
        public bool ServerInfoPopulateEnvironmentVariables { get; set; }
        public bool ServerInfoPopulateConfigs { get; set; }
        public bool EnableMgmtBuilderConfigRoute { get; set; }
        public bool EnableMgmtEchoRoute { get; set; }
        public bool EnableMgmtLogRoute { get; set; }
        public bool EnableMicrosoftServiceDefaultEndpoints { get; set; }
    }

    protected IConfiguration? Configuration { get; private set; }

    protected ILogger? Logger { get; private set; }

    private bool GoCalled;

    protected virtual void SetupConfiguration(WebApplicationBuilder builder, string[] args)
    {
        var configuration = builder.Configuration;
        AssemblySettingsResourceStacking.DiscoverThenStack(configuration, builder.Environment.EnvironmentName, GetType().Assembly, null, Logger);
        SetupConfigurationForRemoteConfigs(builder);
        SetupConfigurationForRemoteSecrets(builder);
        configuration.AddEnvironmentVariables();
        configuration.AddCommandLine(args);
    }

    protected virtual void SetupConfigurationForRemoteConfigs(WebApplicationBuilder builder)
    { }

    protected virtual void SetupConfigurationForRemoteSecrets(WebApplicationBuilder builder)
    { }

    protected virtual void SetupLogging(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.AddConsole();
        loggingBuilder.AddTraceSource(new SourceSwitch("TraceSource", "All"), new ConsoleTraceListener());
        //loggingBuilder.AddOpenTelemetry(options =>
        //{
        //    options.IncludeFormattedMessage = true;
        //    options.ParseStateValues = true;

        //    // ✅ Dapr Logging (Sends logs to Dapr sidecar for distributed tracing)
        //    options.AddOtlpExporter(otlpOptions =>
        //    {
        //        otlpOptions.Endpoint = new Uri("http://localhost:4317"); // Default Dapr OpenTelemetry collector
        //    });
        //});
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()); // Forces OpenAPI schema to use strings for enums when the 
            options.SerializerOptions.Converters.Add(new EnumMemberJsonConverterFactory(true)); // Forces the actual string serialization
        });

        services.UseRevolutionaryStuffApiCore();

        services.AddSingleton<IApplicationNameFinder, HostEnvironmentApplicationNameFinder>();
        // Add services to the container.
        services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

        var c = MyConfig;
        if (c.ConfigureForRazorWebsite)
        {
            services.AddRazorPages();
        }
        else if (c.EnableOpenApi)
        {
            services.AddOpenApi();
            services.Configure<OpenApiOptions>(null, OnConfigureOpenApiOptions);
        }

        // Add services to the container.
        services.AddProblemDetails();
    }

    protected virtual void OnConfigureOpenApiOptions(OpenApiOptions options)
    {
        options.AddOperationTransformer(new OpenApiOperationTransformer());
    }

    protected virtual void ConfigureBuilder(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IHostEnvironment>(builder.Environment);
        builder.AddServiceDefaults();
    }

    protected virtual void SetupMiddleware(WebApplication app)
    {
        app.UseMiddleware<WebApiExceptionMiddleware>();
    }

    private Config MyConfig
    {
        get
        {
            if (field == null)
            {
                Config c = new();
                Configuration?.Bind(Config.ConfigSectionName, c);
                field = c;
            }
            return field;
        }
    }

    protected virtual void MapWebEndpoints(WebApplication app)
    {
        var c = MyConfig;
        if (c.EnableMgmtBuilderConfigRoute)
        {
            app.MapGet("/mgmt/builder", (IOptions<BuilderConfig> configOptions) => configOptions.Value).ManagementApi("BuilderConfig");
        }
        if (c.EnableServerInfoRoute)
        {
            app.MapGet("/mgmt/server", ([FromServices] IServerInfoFinder serverInfoFinder) => serverInfoFinder.GetServerInfo(new() { PopulateConfigs = c.ServerInfoPopulateConfigs, PopulateEnvironmentVariables = c.ServerInfoPopulateEnvironmentVariables })).ManagementApi("ServerInfo");
        }
        if (c.EnableMgmtEchoRoute)
        {
            app.MapGet("/mgmt/echo/{message}", (string message) => $"{message} {message} {message}...").ManagementApi("Echo");
            app.MapPost("/mgmt/echo", async (HttpContext context) =>
            {
                var message = await context.Request.Body.ReadToEndAsync();
                return $"{message} {message} {message}...";
            }).ManagementApi("EchoPost");
        }
        if (c.EnableMgmtLogRoute)
        {
            app.MapGet("/mgmt/log/{message}", (string message, ILogger<ApiProgram> logger, HttpContext context) => { logger.LogWarning(message); context.Response.StatusCode = StatusCodes.Status201Created; }).ManagementApi("LogTrace");
            app.MapPost("/mgmt/log", async (ILogger<ApiProgram> logger, HttpContext context) =>
            {
                var message = await context.Request.Body.ReadToEndAsync();
                logger.LogWarning(message);
                context.Response.StatusCode = StatusCodes.Status201Created;
            }).ManagementApi("LogTracePost");
        }
        if (c.EnableMicrosoftServiceDefaultEndpoints)
        {
            app.MapMicrosoftDefaultEndpoints();
        }
        if (c.ConfigureForRazorWebsite)
        {
            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();
        }
        else
        {
            if (c.EnableIndexRoute)
            {
                app.MapGet("/", (IApplicationNameFinder app) => $"Hi from {app.ApplicationName} at {DateTimeOffset.Now}.");
            }
            if (c.EnableOpenApi)
            {
                app.MapOpenApi();
            }
        }
    }

    private static ILogger CreateStartupLogger()
    {
        using var earlyLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                // Write to System.Diagnostics.Debug — shows up in VS Output → Debug
                .AddDebug()
                // (optional) capture everything ≥ Trace
                .SetMinimumLevel(LogLevel.Trace);
        });
        return earlyLoggerFactory.CreateLogger<ApiProgram>();
    }

    protected virtual void UseHttpsRedirection(WebApplication app)
        => app.UseHttpsRedirection();

    public Task GoAsync(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        Requires.SingleCall(ref GoCalled);

        Stuff.LoggerOfLastResort = Logger = CreateStartupLogger();
        Logger.LogInformation("Starting up - pre app");

        var builder = WebApplication.CreateBuilder(args);

        SetupConfiguration(builder, args);
        Configuration = ((IConfigurationBuilder)builder.Configuration).Build();

        SetupLogging(builder.Logging);

        ConfigureBuilder(builder);

        ConfigureServices(builder.Services);

        var app = builder.Build();

        Stuff.LoggerOfLastResort = Logger = (ILogger)app.Services.GetRequiredService(typeof(ILogger<>).MakeGenericType(GetType()));

        Logger.LogInformation("Starting up - app created");

        app.UseExceptionHandler();

        UseHttpsRedirection(app);

        app.UseAuthorization();

        SetupMiddleware(app);

        MapWebEndpoints(app);

        app.Run();

        return Task.CompletedTask;
    }
}
