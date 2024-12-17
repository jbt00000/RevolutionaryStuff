using Azure.Core;

namespace RevolutionaryStuff.Azure.Services.Authentication;
public interface IAzureTokenCredentialProvider
{
    TokenCredential GetTokenCredential();
}
