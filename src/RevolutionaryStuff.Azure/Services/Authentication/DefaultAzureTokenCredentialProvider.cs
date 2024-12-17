using Azure.Core;
using Azure.Identity;
using RevolutionaryStuff.Core.Services.ApplicationNameFinders;

namespace RevolutionaryStuff.Azure.Services.Authentication;

internal class DefaultAzureTokenCredentialProvider(IApplicationNameFinder ApplicationNameFinder) : IAzureTokenCredentialProvider
{
    TokenCredential IAzureTokenCredentialProvider.GetTokenCredential()
    {
        var name = ApplicationNameFinder.ApplicationName;
        var options = new DefaultAzureCredentialOptions
        {
            Diagnostics =
            {
                ApplicationId = name,                 
#if DEBUG
                IsLoggingContentEnabled = true,
                IsLoggingEnabled = true
#endif
            }
        };
        return new DefaultAzureCredential(options);
    }
}
