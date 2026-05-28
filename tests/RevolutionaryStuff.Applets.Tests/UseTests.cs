using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Applets.Services.TextTemplateRenderers;
using RevolutionaryStuff.Core.ApplicationParts;

#nullable enable

namespace RevolutionaryStuff.Applets.Tests;

[TestClass]
public class UseTests
{
    [TestMethod]
    [DataRow(ServiceLifetime.Singleton)]
    [DataRow(ServiceLifetime.Scoped)]
    [DataRow(ServiceLifetime.Transient)]
    public void AddTextTemplateRenderer_UsesRequestedLifetime(ServiceLifetime serviceLifetime)
    {
        var services = new ServiceCollection();

        services.AddTextTemplateRenderer<ITestTextTemplateRenderer, TestTextTemplateRenderer>(serviceLifetime);

        Assert.AreEqual(serviceLifetime, GetRequiredDescriptor(services, typeof(ITestTextTemplateRenderer)).Lifetime);
        Assert.AreEqual(serviceLifetime, GetRequiredDescriptor(services, typeof(ITextTemplateRenderer)).Lifetime);
    }

    [TestMethod]
    public void UseRevolutionaryStuffApplets_RegistersTextTemplateRenderersAsSingletons()
    {
        var services = new ServiceCollection();

        services.UseRevolutionaryStuffApplets();

        Assert.AreEqual(ServiceLifetime.Singleton, GetRequiredDescriptor(services, typeof(IMustacheTextTemplateRenderer)).Lifetime);
        Assert.AreEqual(ServiceLifetime.Singleton, GetRequiredDescriptor(services, typeof(IScribanTextTemplateRenderer)).Lifetime);
        Assert.AreEqual(2, services.Count(d => d.ServiceType == typeof(ITextTemplateRenderer) && d.Lifetime == ServiceLifetime.Singleton));
    }

    private static ServiceDescriptor GetRequiredDescriptor(IServiceCollection services, Type serviceType)
        => services.Single(d => d.ServiceType == serviceType);

    private interface ITestTextTemplateRenderer : ITextTemplateRenderer;

    private sealed class TestTextTemplateRenderer : ITestTextTemplateRenderer
    {
        Task<string> ITextTemplateRenderer.RenderAsync(string templateText, object templateData, RenderOptions? options)
            => Task.FromResult(templateText);
    }
}
