using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.BlazorWasm.Startup;

namespace RevolutionaryStuff.BlazorWasm.Tests.Startup;

[TestClass]
public class BlazorHostEnvironmentAdapterTests
{
    [TestMethod]
    public void Constructor_MapsWebAssemblyHostEnvironmentProperties()
    {
        TestWebAssemblyHostEnvironment source = new()
        {
            Environment = "Staging",
            BaseAddress = "https://example.test/"
        };

        BlazorHostEnvironmentAdapter adapter = new(source);

        Assert.AreEqual(typeof(BlazorProgram).Assembly.GetName().Name, adapter.ApplicationName);
        Assert.AreEqual("Staging", adapter.EnvironmentName);
        Assert.AreEqual("https://example.test/", adapter.ContentRootPath);
        Assert.IsInstanceOfType<NullFileProvider>(adapter.ContentRootFileProvider);
    }

    [TestMethod]
    public void Register_AddsIHostEnvironmentSingleton()
    {
        TestWebAssemblyHostEnvironment source = new()
        {
            Environment = "Staging",
            BaseAddress = "https://example.test/"
        };
        ServiceCollection services = new();

        services.AddSingleton<IHostEnvironment>(new BlazorHostEnvironmentAdapter(source));

        using var serviceProvider = services.BuildServiceProvider();
        var environment = serviceProvider.GetRequiredService<IHostEnvironment>();

        Assert.AreEqual("Staging", environment.EnvironmentName);
        Assert.AreEqual("https://example.test/", environment.ContentRootPath);
        Assert.AreSame(environment, serviceProvider.GetRequiredService<IHostEnvironment>());
    }

    private sealed class TestWebAssemblyHostEnvironment : IWebAssemblyHostEnvironment
    {
        public string Environment { get; set; }

        public string BaseAddress { get; set; }
    }
}
