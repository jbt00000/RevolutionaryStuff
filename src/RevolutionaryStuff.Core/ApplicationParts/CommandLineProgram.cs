using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.PlatformAbstractions;

namespace RevolutionaryStuff.Core.ApplicationParts
{
    public abstract partial class CommandLineProgram : BaseDisposable
    {
        #region Command Line Args

        protected CommandLineInfo Cli { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public IConfigurationRoot Configuration { get; private set; }

        #endregion

        private void Go() 
            => OnGoAsync().ExecuteSynchronously();

        protected abstract Task OnGoAsync();

        private readonly ManualResetEvent ShutdownRequestedEvent = new ManualResetEvent(false);
        protected WaitHandle ShutdownRequested => ShutdownRequestedEvent;

        protected virtual void OnBuildConfiguration(IConfigurationBuilder builder, string environmentName, bool isDevelopment)
        {
            OnPreBuildConfiguration(builder);

            builder
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{EnvironmentHelpers.GetEnvironmentName()}.json", optional: true)
                .AddEnvironmentVariables();

            OnPostBuildConfiguration(builder);
        }

        protected virtual void OnPreBuildConfiguration(IConfigurationBuilder builder)
        { }

        protected virtual void OnPostBuildConfiguration(IConfigurationBuilder builder)
        { }

        private void BuildConfiguration()
        {
            var env = PlatformServices.Default.Application;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ApplicationBasePath);

            OnBuildConfiguration(builder, EnvironmentHelpers.GetEnvironmentName(), EnvironmentHelpers.CommonEnvironmentNames.Development == EnvironmentHelpers.GetEnvironmentName());

            Configuration = builder.Build();
        }

        protected virtual void OnPostProcessCommandLineArgs()
        { }

        private void ProcessCommandLineArgs(string[] args)
        {
            var cli = new CommandLineInfo(Configuration, args);
            Cli = cli;
            CommandLineSwitchAttribute.SetArgs(Cli, this);
            OnPostProcessCommandLineArgs();
        }

        private IServiceCollection ConfigureOptionsServices;

        protected void ConfigureOptions<TOptions>(string sectionName) where TOptions : class
        {
            Debug.Assert((bool)(this.ConfigureOptionsServices != null));
            OptionsConfigurationServiceCollectionExtensions.Configure<TOptions>(this.ConfigureOptionsServices, this.Configuration.GetSection(sectionName));
        }

        protected virtual void OnConfigureServices(IServiceCollection services)
        { }

        private void ConfigureServices()
        {
            ConfigureOptionsServices = new ServiceCollection();
            try
            {
                ConfigureOptionsServices.Add(new ServiceDescriptor(typeof(IConfiguration), Configuration));
                ConfigureOptionsServices.AddOptions();
                ConfigureLogging(ConfigureOptionsServices);
                OnConfigureServices(ConfigureOptionsServices);
                ServiceProvider = ConfigureOptionsServices.BuildServiceProvider();
            }
            finally
            {
                this.ConfigureOptionsServices = null;
            }
        }

        private void ConfigureLogging(IServiceCollection services)
        {
            var loggingFactory = new LoggerFactory();
            OnConfigureLogging(services, loggingFactory);
            services.AddSingleton(loggingFactory);
            services.AddLogging();
        }

        protected virtual void OnConfigureLogging(IServiceCollection services, ILoggerFactory loggerFactory)
        {
            var mon = new HardcodedOptionsMonitor<ConsoleLoggerOptions>(new ConsoleLoggerOptions());
            var p = new ConsoleLoggerProvider(mon);
            loggerFactory.AddProvider(p);
        }

        protected CommandLineProgram()
        { }

        public static void PrintUsage(Type t)
        {
            var usage = CommandLineInfo.GetUsage(t);
            Trace.WriteLine(usage);
        }

        public static void Main<TCommandLineProgram>(string[] args) where TCommandLineProgram : CommandLineProgram
        {
            Trace.Listeners.Add(new Diagnostics.ConsoleTraceListener());
            CommandLineProgram p = null;
            bool programInOperation = false;
            try
            {
                var ci = typeof(TCommandLineProgram).GetTypeInfo().GetConstructor(Empty.TypeArray);
                p = (CommandLineProgram)ci.Invoke(Empty.ObjectArray);
                p.BuildConfiguration();
                p.ProcessCommandLineArgs(args);
                p.ConfigureServices();
                programInOperation = true;
                p.Go();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException is CommmandLineInfoException)
                {
                    Trace.TraceError(ex.InnerException.Message);
                }
                else if (ex is CommmandLineInfoException)
                {
                    Trace.TraceError(ex.Message);
                }
                else
                {
                    Trace.TraceError(ex.Message);
                }
                if (!programInOperation)
                {
                    PrintUsage(typeof(TCommandLineProgram));
                }
            }
            finally
            {
                Stuff.Dispose(p);
                Stuff.Cleanup();
            }
        }
    }
}
