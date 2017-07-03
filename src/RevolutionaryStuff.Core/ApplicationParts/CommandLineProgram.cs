using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Threading.Tasks;
using Microsoft.Extensions.PlatformAbstractions;
using System.Threading;

namespace RevolutionaryStuff.Core.ApplicationParts
{
    public abstract class CommandLineProgram : BaseDisposable
    {
        #region Command Line Args

        protected CommandLineInfo Cli { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public IConfigurationRoot Configuration { get; private set; }

        #endregion

        private void Go() => OnGoAsync().ExecuteSynchronously();

        protected abstract Task OnGoAsync();

        private readonly ManualResetEvent ShutdownRequestedEvent = new ManualResetEvent(false);
        protected WaitHandle ShutdownRequested => ShutdownRequestedEvent;

        protected virtual void OnBuildConfiguration(IConfigurationBuilder builder)
        { }

        private void BuildConfiguration()
        {
            var env = PlatformServices.Default.Application;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ApplicationBasePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            OnBuildConfiguration(builder);

            builder.AddEnvironmentVariables();
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

        protected virtual void OnConfigureServices(IServiceCollection services)
        { }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();
            services.Add(new ServiceDescriptor(typeof(IConfiguration), Configuration));
            services.AddOptions();
            OnConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
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
            CommandLineProgram p = null;
            bool programInOperation = false;
            try
            {
                var ci = typeof(TCommandLineProgram).GetTypeInfo().GetConstructor(Empty.TypeArray);
                p = (CommandLineProgram)ci.Invoke(Empty.ObjectArray);
                programInOperation = true;
                p.BuildConfiguration();
                p.ProcessCommandLineArgs(args);
                p.ConfigureServices();
                p.Go();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException is CommmandLineInfoException)
                {
                    Trace.WriteLine(ex.InnerException.Message);
                }
                else if (ex is CommmandLineInfoException)
                {
                    Trace.WriteLine(ex.Message);
                }
                else
                {
                    Trace.WriteLine(ex);
                }
                if (!programInOperation)
                {
                    PrintUsage(typeof(TCommandLineProgram));
                }
            }
            finally
            {
                Stuff.Dispose(p);
            }
        }
    }
}
