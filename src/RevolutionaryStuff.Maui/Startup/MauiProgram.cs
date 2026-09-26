using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Maui.Startup;

public abstract class MauiProgram
{
    private class Config
    {
        public const string ConfigSectionName = "MauiProgram";
    }

    protected IConfiguration Configuration { get; private set; }

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

    protected virtual void SetupConfiguration(MauiAppBuilder builder)
    {
        var configuration = builder.Configuration;
        AssemblySettingsResourceStacking.DiscoverThenStack(configuration, builder.Environment.EnvironmentName, GetType().Assembly, null, Logger);
        SetupConfigurationForRemoteConfigs(builder);
        SetupConfigurationForRemoteSecrets(builder);
        configuration.AddEnvironmentVariables();
    }

    protected virtual void SetupConfigurationForRemoteConfigs(MauiAppBuilder builder)
    { }

    protected virtual void SetupConfigurationForRemoteSecrets(MauiAppBuilder builder)
    { }

    protected virtual void SetupLogging(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.AddConsole();
        //loggingBuilder.AddTraceSource(new SourceSwitch("TraceSource", "All"), new ConsoleTraceListener());
    }

    private static ILogger CreateStartupLogger()
    {
        using var earlyLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                // Write to System.Diagnostics.Debug — shows up in VS Output → Debug
                //.AddDebug()
                // (optional) capture everything ≥ Trace
                .SetMinimumLevel(LogLevel.Trace);
        });
        return earlyLoggerFactory.CreateLogger<MauiProgram>();
    }

    protected virtual void ConfigureBuilder(MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IHostEnvironment>(builder.Environment);
        //        builder.AddServiceDefaults();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.UseRevolutionaryStuffCore();// settings?.RevolutionaryStuffCoreUseSettings);
    }

    public MauiApp CreateMauiApp()
    {
        Requires.SingleCall(ref GoCalled);

        Stuff.LoggerOfLastResort = Logger = CreateStartupLogger();
        Logger.LogInformation("Starting up - pre app");


        var builder = MauiApp.CreateBuilder();

        SetupConfiguration(builder);
        Configuration = ((IConfigurationBuilder)builder.Configuration).Build();

        SetupLogging(builder.Logging);

        ConfigureBuilder(builder);

        ConfigureServices(builder.Services);

        var app = builder.Build();

        Stuff.LoggerOfLastResort = Logger = (ILogger)app.Services.GetRequiredService(typeof(ILogger<>).MakeGenericType(GetType()));

        Logger.LogInformation("Starting up - app created");

        return app;
    }

}
