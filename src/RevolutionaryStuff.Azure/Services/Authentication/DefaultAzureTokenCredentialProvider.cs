using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core.Services.ApplicationNameFinders;

namespace RevolutionaryStuff.Azure.Services.Authentication;

internal class DefaultAzureTokenCredentialProvider(IOptions<DefaultAzureTokenCredentialProvider.Config> ConfigOptions, IApplicationNameFinder ApplicationNameFinder) : IAzureTokenCredentialProvider
{
    public class Config
    {
        public const string ConfigSectionName = "DefaultAzureTokenCredentialProvider";
        public int MaxApplicationNameLength { get; set; } = 24;
        public bool EnableLogging { get; set; }
    }

    TokenCredential IAzureTokenCredentialProvider.GetTokenCredential()
    {
        var config = ConfigOptions.Value;
        var name = ApplicationNameFinder?.ApplicationName.TrimOrNull();
        if (name != null && name.Length > config.MaxApplicationNameLength)
        {
            name = StringHelpers.TruncateWithMidlineEllipsis(name, config.MaxApplicationNameLength);
        }
        var options = new DefaultAzureCredentialOptions
        {
            Diagnostics =
            {
                ApplicationId = name,
                IsTelemetryEnabled = name!=null || config.EnableLogging,
                IsLoggingContentEnabled = config.EnableLogging,
                IsLoggingEnabled = config.EnableLogging,
                IsAccountIdentifierLoggingEnabled = config.EnableLogging,
            }
        };
        return new DefaultAzureCredential(options);
    }
}
