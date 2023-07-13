using System.Diagnostics;
using Newtonsoft.Json;

namespace RevolutionaryStuff.Core.ApplicationParts;
public static class ServiceUseManager
{
    private class Usage
    {
        public string SettingsJson { get; init; }
        public StackTrace UsedFrom { get; init; }
    }

    public enum PriorUsageRulesEnum { Disallow, SameSettings, Allow }

    private static readonly IDictionary<Type, Usage> UsageByUseType = new Dictionary<Type, Usage>();

    public static void Use<TSettings>(TSettings settings, Action initialize, PriorUsageRulesEnum rule = PriorUsageRulesEnum.SameSettings)
    {
        ArgumentNullException.ThrowIfNull(initialize);

        var settingsType = typeof(TSettings);

        var settingsJson = settings == null ? null : JsonConvert.SerializeObject(settings);
        if (UsageByUseType.ContainsKey(settingsType))
        {
            var usage = UsageByUseType[settingsType];
            switch (rule)
            {
                case PriorUsageRulesEnum.Allow:
                    return;
                case PriorUsageRulesEnum.Disallow:
                    throw new($"{settingsType} already used and re-usage disallowed.\n{usage.UsedFrom}\n{new StackTrace()}");
                case PriorUsageRulesEnum.SameSettings:
                    if (usage.SettingsJson == settingsJson)
                    {
                        return;
                    }

                    throw new($"{settingsType} already used but with different settings.\n{usage.UsedFrom}\n{new StackTrace()}");
                default:
                    throw new UnexpectedSwitchValueException(rule);
            }
        }

        UsageByUseType[settingsType] = new()
        {
            SettingsJson = settingsJson,
            UsedFrom = new()
        };
        initialize();
    }
}
