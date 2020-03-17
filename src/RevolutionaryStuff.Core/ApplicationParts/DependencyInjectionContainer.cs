using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.PlatformAbstractions;

namespace RevolutionaryStuff.Core.ApplicationParts
{
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

        public void Initialize()
        {
            Requires.SingleCall(ref InitCalled);
            BuildConfiguration();
            ConfigureServices();
        }

        private void BuildConfiguration()
        {
            var env = PlatformServices.Default.Application;

            var builder = new ConfigurationBuilder().SetBasePath(env.ApplicationBasePath);

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


        /// <remarks>ONLY call this from OnConfigureServices</remarks>
        protected void ConfigureOptions<TOptions>(string sectionName) where TOptions : class
        {
            Debug.Assert(ConfigureOptionsServices != null);
            ConfigureOptionsServices.Configure<TOptions>(Configuration.GetSection(sectionName));
        }

        private IServiceCollection ConfigureOptionsServices;

        private void ConfigureServices()
        {
            var services = new ServiceCollection();
            ConfigureOptionsServices = services;
            try
            {
                services.Add(new ServiceDescriptor(typeof(IConfiguration), Configuration));
                services.AddOptions();
                //ConfigureLogging(services);
                OnConfigureServices(services);
                ServiceProvider = services.BuildServiceProvider();
            }
            finally
            {
                ConfigureOptionsServices = null;
            }
        }

        protected virtual void OnConfigureServices(IServiceCollection services)
        { }

        public virtual IServiceProvider CreateScopedProvider()
            => ServiceProvider.CreateScope().ServiceProvider;
    }
}
