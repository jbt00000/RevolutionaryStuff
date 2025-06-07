namespace RevolutionaryStuff.Storage;

public enum StorageProviderExceptionCodes
{
    CannotDeleteRootFolder,
    NotWithinTree,
    CannotCreateFile,
    CannotCreateFileWhenItAlreadyExists,
    DoesNotExist,
}

public class StorageProviderException : CodedException<StorageProviderExceptionCodes>
{
    public StorageProviderException(StorageProviderExceptionCodes code)
        : base(code)
    { }

    public StorageProviderException(StorageProviderExceptionCodes code, Exception inner)
        : base(code, inner)
    { }

    public StorageProviderException(StorageProviderExceptionCodes code, string message)
        : base(code, message)
    { }
}
