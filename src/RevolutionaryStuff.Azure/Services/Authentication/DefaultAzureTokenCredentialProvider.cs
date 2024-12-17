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

        public bool EnableLogging { get; set; }
    }

    TokenCredential IAzureTokenCredentialProvider.GetTokenCredential()
    {
        var name = ApplicationNameFinder?.ApplicationName.TrimOrNull();
        var config = ConfigOptions.Value;
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
