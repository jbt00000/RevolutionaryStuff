using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Storage.Implementation.Base;

namespace RevolutionaryStuff.Storage.Providers.LocalFileSystem;

public partial class PhysicalStorageProvider : BaseStorageProvider, IPhysicalStorageProvider
{
    private readonly IOptions<Config> ConfigOptions;

    public static PhysicalStorageProvider CreateWithRootFolder(string rootFolder, ILogger<PhysicalStorageProvider> logger)
    {
        var configOptions = new OptionsWrapper<Config>(
            new Config
            {
                RootFolder = rootFolder
            });
        return new PhysicalStorageProvider(configOptions, logger);
    }

    internal PhysicalStorageProvider(PhysicalStorageProvider sp, string absolutePath)
        : this(sp.ConfigOptions, absolutePath, sp.Logger)
    { }

    public PhysicalStorageProvider(IOptions<Config> configOptions, ILogger<PhysicalStorageProvider> logger)
        : this(configOptions, null, logger)
    { }

    private PhysicalStorageProvider(IOptions<Config> configOptions, string absolutePath, ILogger logger)
        : base(logger)
    {
        ConfigOptions = configOptions;
        //Don't want the root changing while we're instantiated!
        var config = configOptions.Value;
        CaseSensitive = config.CaseSensitive.GetValueOrDefault(Environment.OSVersion.Platform == PlatformID.Win32NT);
        InternalFolderSeparatorChar = (config.FolderSeparator ?? Path.DirectorySeparatorChar.ToString()).TrimOrNull()[0];
        AbsolutePath = NormalizeExternalFolderPath(absolutePath ?? Environment.ExpandEnvironmentVariables(configOptions.Value.RootFolder));
        RootFolder = new PhysicalFolderEntry(this, new DirectoryInfo(AbsolutePath));

        LogDebug("{caseSensitive} {internalFolderSeparatorChar} [{absolutePath}] [{rootFolder}]", CaseSensitive, InternalFolderSeparatorChar, AbsolutePath, RootFolder);
    }

    protected override Task<IFolderEntry> OnOpenRootFolderAsync()
        => Task.FromResult(RootFolder);

    private IFolderEntry RootFolder { get; }
}
