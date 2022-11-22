using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RevolutionaryStuff.Core.ApplicationParts;

public abstract class DependencyInjectionContainer : BaseDisposable
{
    private const string AppSettingsJsonFileName = "appsettings.json";
    public IServiceProvider ServiceProvider { get; private set; }

    public IConfigurationRoot Configuration { get; private set; }

    protected DependencyInjectionContainer(Type userSecretsAssemblyType = null)
    {
        UserSecretsAssemblyType = userSecretsAssemblyType;
    }

    private bool InitCalled;
    private readonly Type UserSecretsAssemblyType;

    public virtual void Initialize()
    {
        Requires.SingleCall(ref InitCalled);
        BuildConfiguration();
        ConfigureServices();
    }

    private void BuildConfiguration()
    {
        var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory);

        OnPreBuildConfiguration(builder);
        Configuration = builder.Build();
        OnBuildConfiguration(builder, EnvironmentHelpers.GetEnvironmentName(), EnvironmentHelpers.CommonEnvironmentNames.Development == EnvironmentHelpers.GetEnvironmentName());
        Configuration = builder.Build();
        OnPostBuildConfiguration(builder);
        Configuration = builder.Build();
    }

    protected virtual void OnBuildConfiguration(IConfigurationBuilder builder, string environmentName, bool isDevelopment)
    {
        var fn = Path.GetFullPath(AppSettingsJsonFileName);
        if (!File.Exists(fn))
        {
            var dir = Path.GetDirectoryName(typeof(DependencyInjectionContainer).Assembly.Location);
            fn = Path.Combine(dir, AppSettingsJsonFileName);
            if (File.Exists(fn))
            {
                Trace.WriteLine($"Switching builder base path to {dir}");
                builder.SetBasePath(dir);
            }
        }

        builder
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{EnvironmentHelpers.GetEnvironmentName()}.json", optional: true)
            .AddEnvironmentVariables();
        if (UserSecretsAssemblyType != null)
        {
            builder.AddUserSecrets(UserSecretsAssemblyType.Assembly, true);
        }
        Configuration = builder.Build();
    }

    protected virtual void OnPreBuildConfiguration(IConfigurationBuilder builder)
    { }

    protected virtual void OnPostBuildConfiguration(IConfigurationBuilder builder)
    { }

    private void ConfigureServices()
    {
        var services = new ServiceCollection
        {
            new ServiceDescriptor(typeof(IConfiguration), Configuration)
        };
        services.AddOptions();
        //ConfigureLogging(services);
        OnConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    protected virtual void OnConfigureServices(IServiceCollection services)
    { }

    public virtual IServiceProvider CreateScopedProvider()
        => ServiceProvider.CreateScope().ServiceProvider;
}
