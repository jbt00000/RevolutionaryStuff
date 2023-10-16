using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace RevolutionaryStuff.Core.ApplicationParts;

public class RevolutionaryStuffCoreConfig
{
    public const string ConfigSectionName = "RevolutionaryStuffCoreOptions";

    public string ApplicationName { get; set; }

    public static string GetApplicationName(IConfiguration config, IHostEnvironment he = null)
        => StringHelpers.Coalesce(config[ConfigSectionName + ":" + nameof(ApplicationName)], he?.ApplicationName, System.Reflection.Assembly.GetEntryAssembly()?.GetName()?.Name, Process.GetCurrentProcess().ProcessName);
}
