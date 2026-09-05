using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.Applets.Services.Runners;
using RevolutionaryStuff.Core.Services.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RevolutionaryStuff.Applets.Tests;

[TestClass]
public class ScheduledRunnerServiceCollectionExtensionsTests
{
    [TestMethod]
    public void AddScheduledRunnerHost_RegistersIndependentHostsAndConfigs()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["First:Schedule"] = "0 1 * * *",
            ["First:AllTenants"] = "true",
            ["Second:Schedule"] = "30 2 * * *",
            ["Second:AllTenants"] = "false"
        });
        var services = CreateServices();

        services
            .AddScheduledRunnerHost<FirstRunner>(configuration, "First")
            .AddScheduledRunnerHost<SecondRunner>(configuration, "Second");

        using var serviceProvider = services.BuildServiceProvider();
        var configOptions = serviceProvider.GetRequiredService<IOptionsMonitor<ScheduledRunnerHostConfig>>();
        var hosts = serviceProvider.GetServices<IHostedService>().ToList();

        Assert.AreEqual("0 1 * * *", configOptions.Get(typeof(FirstRunner).FullName).Schedule);
        Assert.IsTrue(configOptions.Get(typeof(FirstRunner).FullName).AllTenants);
        Assert.AreEqual("30 2 * * *", configOptions.Get(typeof(SecondRunner).FullName).Schedule);
        Assert.IsFalse(configOptions.Get(typeof(SecondRunner).FullName).AllTenants);
        Assert.AreEqual(2, hosts.Count);
    }

    [TestMethod]
    public void AddScheduledRunnerHost_RejectsInvalidCronSchedule()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["Invalid:Schedule"] = "not a cron expression"
        });
        var services = CreateServices();
        services.AddScheduledRunnerHost<FirstRunner>(configuration, "Invalid");

        using var serviceProvider = services.BuildServiceProvider();
        var configOptions = serviceProvider.GetRequiredService<IOptionsMonitor<ScheduledRunnerHostConfig>>();

        Assert.ThrowsExactly<OptionsValidationException>(() => configOptions.Get(typeof(FirstRunner).FullName));
    }

    private static IConfiguration CreateConfiguration(IDictionary<string, string> values)
        => new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private static ServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ITenantIdEnumerator, FakeTenantIdEnumerator>();
        return services;
    }

    private sealed class FirstRunner : IRunner
    {
        public Task RunAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class SecondRunner : IRunner
    {
        public Task RunAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeTenantIdEnumerator : ITenantIdEnumerator
    {
        public Task<IList<string>> GetTenantIdsAsync()
            => Task.FromResult<IList<string>>([]);

        public Task ForEachScopedTenantAsync(Func<ITenantIdEnumerator.ExecuteArgs, Task> executeAsync)
            => Task.CompletedTask;
    }
}
