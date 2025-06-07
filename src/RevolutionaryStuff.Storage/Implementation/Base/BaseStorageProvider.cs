using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Storage.Implementation.Base;

public abstract class BaseStorageProvider : BaseLoggingDisposable, IStorageProvider
{
    internal ILogger StorageProviderLogger => Logger;
    protected bool CaseSensitive { get; set; }
    public char InternalFolderSeparatorChar { get; protected set; } = StorageHelpers.ExternalFolderSeparatorChar;
    public string InternalFolderSeparatorString => InternalFolderSeparatorChar.ToString();

    public string AbsolutePath { get; protected set; }

    protected BaseStorageProvider(ILogger logger)
        : base(logger)
    { }

    public string InternalizePath(string path)
        => path.Replace(StorageHelpers.ExternalFolderSeparatorChar, InternalFolderSeparatorChar);

    public string ExternalizePath(string path)
        => path.Replace(InternalFolderSeparatorChar, StorageHelpers.ExternalFolderSeparatorChar);

    public bool StringEquals(string a, string b)
        => 0 == string.Compare(a, b, !CaseSensitive);

    public bool StringStartsWith(string s, string test)
        => s.StartsWith(test, CaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase);

    public string NormalizeExternalFolderPath(string path)
    {
        var ret = path.Replace('/', StorageHelpers.ExternalFolderSeparatorChar).Replace('\\', StorageHelpers.ExternalFolderSeparatorChar);
        if (!ret.EndsWith(StorageHelpers.ExternalFolderSeparator))
            ret += StorageHelpers.ExternalFolderSeparatorChar;
#if DEBUG
        if (path != "/")
        {
            LogDebug(nameof(NormalizeExternalFolderPath) + " [{path}]=>=[{ret}] ", path, ret);
        }
#endif
        return ret;
    }

    public virtual string CombinePaths(string basePath, string addedPath)
    {
        var left = NormalizeExternalFolderPath(basePath);
        var segments = GetPathSegments(addedPath, true);
        var ret = left + segments.Join(StorageHelpers.ExternalFolderSeparator);
        ret = InternalizePath(ret);
#if DEBUG
        LogDebug(nameof(CombinePaths) + " [{basePath}]+[{addedPath}]=[{ret}] ", left, addedPath, ret);
#endif
        return ret;
    }

    public void RequiresForwardOnlyValidPath(string path, string argName)
    {
        Requires.Text(path, argName);
        GetPathSegments(path, true);
    }

    public virtual string GetRootRelativePath(string absolutePath)
    {
        var root = StorageHelpers.GetPathSegments(AbsolutePath);
        var item = StorageHelpers.GetPathSegments(absolutePath);
        return $"{StorageHelpers.RootPath}{item.Skip(root.Length).Join(StorageHelpers.RootPath)}";
    }

    public string[] GetPathSegments(string path, bool throwOnInvalidParts = true)
        => StorageHelpers.GetPathSegments(path, throwOnInvalidParts);

    public async Task<IFolderEntry> OpenRootFolderAsync()
    {
        if (RootFolder == null)
        {
            using (CreateLogRegion(nameof(OpenRootFolderAsync)))
            {
                RootFolder = await OnOpenRootFolderAsync();
            }
        }
        return RootFolder;
    }
    private IFolderEntry RootFolder;

    protected abstract Task<IFolderEntry> OnOpenRootFolderAsync();
}
