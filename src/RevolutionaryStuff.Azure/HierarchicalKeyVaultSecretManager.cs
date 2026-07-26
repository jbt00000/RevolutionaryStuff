using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace RevolutionaryStuff.Azure;

public class HierarchicalKeyVaultSecretManager : KeyVaultSecretManager
{
    public const string DefaultPrefixSeparator = "-";
    public const string DefaultSegmentSeparator = "--";

    public string PrefixSeparator { get; } = DefaultPrefixSeparator;
    public string SegmentSeparator { get; } = DefaultSegmentSeparator;

    private readonly string AppNamePrefix;
    private readonly Func<string, string> KeyNameTransformer;

    public HierarchicalKeyVaultSecretManager(string appName = null, Func<string, string> keyNameTransformer = null)
    {
        appName = appName.TrimOrNull();
        if (appName == null)
        {
            AppNamePrefix = "";
        }
        else
        {
            AppNamePrefix = appName + PrefixSeparator;
        }
        KeyNameTransformer = keyNameTransformer;
    }

    public override string GetKey(KeyVaultSecret secret)
    {
        var name = secret.Name[AppNamePrefix.Length..].Replace(SegmentSeparator, ConfigurationPath.KeyDelimiter);
        if (name.ToLower().Contains("apikey"))
        {
            Stuff.NoOp(secret);
        }
        name = KeyNameTransformer?.Invoke(name) ?? name;
        return name;
    }

    public override bool Load(SecretProperties secret)
        => secret.Enabled == true && (string.IsNullOrEmpty(AppNamePrefix) || secret.Name.StartsWith(AppNamePrefix));
}


