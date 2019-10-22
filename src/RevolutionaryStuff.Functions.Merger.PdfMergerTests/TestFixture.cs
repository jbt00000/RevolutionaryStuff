using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Mergers;
using RevolutionaryStuff.Mergers.Pdf;

namespace RevolutionaryStuff.Functions.Merger.PdfMergerTests
{
    [TestClass]
    public class TestFixture : DependencyInjectionContainer
    {
        public static TestFixture Instance { get; private set; }

        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
            Instance = new TestFixture();
            Instance.Initialize();
        }

        protected override void OnConfigureServices(IServiceCollection services)
        {
            base.OnConfigureServices(services);

            services.Configure<PdfMerger.Config>(Configuration.GetSection(PdfMerger.Config.ConfigSectionName));

            services.AddScoped<IPdfMerger, PdfMerger>();
        }

        public IServiceProvider CreateScopedProvider()
            => ServiceProvider.CreateScope().ServiceProvider;
    }
}
