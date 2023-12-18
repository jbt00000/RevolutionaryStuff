﻿using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Configuration;

namespace RevolutionaryStuff.Core.ApplicationParts;

public class ConfigStackSettings
{
    private const string ConfigSettingsJsonFileName = "configsettings.json";
    public const string ConfigSectionName = "ConfigStackSettings";

    public class ResourceInfo
    {
        public string AssemblyName { get; set; }
        public string BaseConfigFileName { get; set; }
        public override string ToString()
            => $"assembly=>{AssemblyName} baseConfig=>{BaseConfigFileName}";
    }

    public List<ResourceInfo> Resources { get; set; }

    public static void Add(IConfigurationBuilder builder, string environmentName, Assembly configSettingsAssembly = null, string configSettingsResourceName = null)
    {
        var a = configSettingsAssembly ?? Assembly.GetEntryAssembly();
        ArgumentNullException.ThrowIfNull(a, $"You must either pass in a {nameof(configSettingsAssembly)} OR be on a platform where Assembly.GetEntryAssembly() functions");
        var st = a.GetEmbeddedResourceAsStream(configSettingsResourceName ?? ConfigSettingsJsonFileName);
        if (st != null)
        {
            builder.AddJsonStream(st);
        }
        var c = builder.Build();
        var css = c.Get<ConfigStackSettings>(ConfigSectionName);
        css.Add(builder, environmentName);
    }

    private void Add(IConfigurationBuilder builder, string environmentName)
    {
        Requires.Text(environmentName);

        var assemblyByName = AssemblyLoadContext.Default.Assemblies.ToDictionary(a => a.GetName().Name, Comparers.CaseInsensitiveStringComparer);

        void AddJsonStream(Assembly a, string name, bool required)
        {
            var st = a.GetEmbeddedResourceAsStream(name);
            if (st != null)
            {
                builder.AddJsonStream(st);
                Trace.WriteLine($"{nameof(ConfigStackSettings)}: Stacking json from assembly {a.GetName().Name} of resource {name}");
            }
            else if (required)
            {
                throw new Exception($"{nameof(ConfigStackSettings)}: Required json config resource [{name}] not found");
            }
        }

        foreach (var info in Resources)
        {
            var a = assemblyByName.GetValue(info.AssemblyName);
            if (a == null)
            {
                a = Assembly.Load(info.AssemblyName);
                if (a == null)
                {
                    throw new Exception($"{nameof(ConfigStackSettings)}: cannot find assembly {info.AssemblyName}");
                }
            }
            var baseName = info.BaseConfigFileName ?? "appsettings";
            AddJsonStream(a, $"{baseName}.json", true);
            AddJsonStream(a, $"{baseName}.{environmentName}.json", false);
            AddJsonStream(a, $"{baseName}.builder.json", false);
#if DEBUG
            AddJsonStream(a, $"{baseName}.local.json", false);
#endif
        }
    }
}
