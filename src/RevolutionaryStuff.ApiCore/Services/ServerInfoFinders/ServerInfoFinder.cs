using System.Collections;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RevolutionaryStuff.Core.Services.ApplicationNameFinders;

namespace RevolutionaryStuff.ApiCore.Services.ServerInfoFinders;

internal class ServerInfoFinder(IApplicationNameFinder _applicationNameFinder, IHostEnvironment _hostEnvironment, IConfiguration _configuration) : IServerInfoFinder
{
    ServerInfo IServerInfoFinder.GetServerInfo(ServerInfoOptions? options)
    {
        options ??= ServerInfoOptions.Default;

        Dictionary<string, string?>? environmentVariables = null;
        if (options.PopulateEnvironmentVariables)
        {
            environmentVariables = [];
            foreach (DictionaryEntry kvp in Environment.GetEnvironmentVariables())
            {
                var key = kvp.Key?.ToString();
                if (key == null)
                    continue;

                environmentVariables[key] = kvp.Value?.ToString();
            }
        }

        IDictionary<string, string?>? configs = null;
        if (options.PopulateConfigs)
            configs = new Dictionary<string, string?>(_configuration.AsEnumerable());

        return new ServerInfo()
        {
            ApplicationStartedAt = Stuff.ApplicationStartedAt,
            ApplicationInstanceId = Stuff.ApplicationInstanceId,
            MachineName = Environment.MachineName,
            ServerTime = DateTimeOffset.UtcNow,
            OperatingSystemVersion = new(Environment.OSVersion),
            Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
            Is64BitProcess = Environment.IsPrivilegedProcess,
            EntryPointAssembly = Assembly.GetEntryAssembly()?.FullName,
            ApplicationName = _applicationNameFinder.ApplicationName,
            EnvironmentName = _hostEnvironment.EnvironmentName,
            EnvironmentVariables = environmentVariables?.AsReadOnlyDictionary(),
            Configs = configs?.AsReadOnlyDictionary(),
            TargetFramework = AppDomain.CurrentDomain?.SetupInformation?.TargetFrameworkName
        };
    }
}
