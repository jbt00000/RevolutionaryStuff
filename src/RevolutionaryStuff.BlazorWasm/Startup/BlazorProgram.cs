using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.BlazorWasm.Startup;

public abstract class BlazorProgram
{
    private class Config
    {
        public const string ConfigSectionName = "MauiProgram";
    }

    private IConfiguration Configuration { get; set; }

    protected ILogger Logger { get; private set; }

    private bool GoCalled;

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

    protected virtual void SetupConfiguration(WebAssemblyHostBuilder builder)
    {
        var configuration = builder.Configuration;
        AssemblySettingsResourceStacking.DiscoverThenStack(configuration, builder.HostEnvironment.Environment, GetType().Assembly, null, Logger);
        SetupConfigurationForRemoteConfigs(builder);
        SetupConfigurationForRemoteSecrets(builder);
        configuration.AddEnvironmentVariables();
    }

    protected virtual void SetupConfigurationForRemoteConfigs(WebAssemblyHostBuilder builder)
    { }

    protected virtual void SetupConfigurationForRemoteSecrets(WebAssemblyHostBuilder builder)
    { }

    protected virtual void SetupLogging(ILoggingBuilder loggingBuilder)
    {
#if DEBUG
        loggingBuilder.AddDebug();
#endif

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
        return earlyLoggerFactory.CreateLogger<BlazorProgram>();

    }

    protected virtual void ConfigureBuilder(WebAssemblyHostBuilder builder)
    {
        Stuff.NoOp(builder.HostEnvironment);

        builder.Services.AddSingleton<IHostEnvironment>(new BlazorHostEnvironmentAdapter(builder.HostEnvironment));

        //        builder.AddServiceDefaults();
#if DEBUG
        builder.Logging.AddDebug();
#endif
    }

    protected virtual void ConfigureServices(WebAssemblyHostBuilder builder)
    {
        ConfigureServices(builder.Services);
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.UseRevolutionaryStuffCore();
    }

    protected virtual void ConfigureHost(WebAssemblyHost host)
    {
    }

    public WebAssemblyHost Go(string[] args)
    {
        Requires.SingleCall(ref GoCalled);

        Stuff.LoggerOfLastResort = Logger = CreateStartupLogger();
        Logger.LogInformation("Starting up - pre app");

        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        SetupConfiguration(builder);

        Configuration = ((IConfigurationBuilder)builder.Configuration).Build();

        SetupLogging(builder.Logging);

        ConfigureBuilder(builder);

        ConfigureServices(builder);

        var app = builder.Build();

        Stuff.LoggerOfLastResort = Logger = (ILogger)app.Services.GetRequiredService(typeof(ILogger<>).MakeGenericType(GetType()));

        Logger.LogInformation("Starting up - app created");

        var host = builder.Build();

        ConfigureHost(host);

        return host;
    }
}
