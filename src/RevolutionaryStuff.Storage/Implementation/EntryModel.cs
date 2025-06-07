namespace RevolutionaryStuff.Storage.Implementation;

public class EntryModel
{
    public string Token { get; set; } //Guid or encrypted string that contains {referncetoStorageProvider, storageProviderRoot}

    public DateTimeOffset LastModified { get; set; }

    public string Name { get; set; }

    public string Path { get; set; }

    public bool IsFolder { get; set; }

    public bool IsFile => !IsFolder;

    #region For Files
    #endregion

    #region For Folders

    public long? Length { get; set; }

    #endregion

    public EntryModel()
    { }

    public EntryModel(IEntry entry)
    {
        try
        {
            LastModified = entry.LastModified;
        }
        catch (NotSupportedException)
        {
            LastModified = DateTimeOffset.MinValue;
        }
        Name = entry.Name;
        Path = entry.Path;
        if (entry is IFileEntry file)
            Length = file.Length;
        else if (entry is IFolderEntry)
        {
            IsFolder = true;
        }
    }
}
