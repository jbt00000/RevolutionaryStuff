using System.IO;
using RevolutionaryStuff.Storage.Implementation;
using Traffk.StorageProviders.Providers.PhysicalStorage;

namespace RevolutionaryStuff.Storage.Providers.LocalFileSystem;

internal class PhysicalFindResults : IFindResults
{
    private const string AllMatchSearch = "*.*";

    private readonly List<IEntry> AllEntries = [];

    private readonly List<IEntry> SlicedEntries;

    private readonly PhysicalStorageProvider StorageProvider;

    public PhysicalFindResults(
        PhysicalStorageProvider storageProvider,
        string searchRootPath,
        IFindCriteria criteria
    )
    {
        criteria ??= FindCriteria.DefaultFindCriteria;

        StorageProvider = storageProvider;

        if (Directory.Exists(searchRootPath))
        {
            foreach (var fullPath in Directory.GetFileSystemEntries(searchRootPath, AllMatchSearch, ToSearchOption(criteria)))
            {
                PopulateResults(storageProvider, criteria, fullPath);
            }
        }

        PageNumber = 0;
        SlicedEntries = AllEntries
            .Skip(PageNumber * PageSize)
            .Take(PageSize)
            .ToList();
    }

    private PhysicalFindResults(PhysicalFindResults fr, int pageNumber)
    {
        StorageProvider = fr.StorageProvider;
        PageNumber = pageNumber;
        AllEntries = fr.AllEntries;

        SlicedEntries = AllEntries
            .Skip(PageNumber * PageSize)
            .Take(PageSize)
            .ToList();
    }

    private int PageNumber { get; }

    IList<IEntry> IFindResults.Entries
        => SlicedEntries;

    private const int PageSize = 100;

    Task<IFindResults> IFindResults.NextAsync()
        => Task.FromResult<IFindResults>(
            new PhysicalFindResults(this, PageNumber + 1));

    private SearchOption ToSearchOption(IFindCriteria criteria)
    {
        return criteria.NestingOption switch
        {
            MatchNestingOptionEnum.CurrentFolder => SearchOption.TopDirectoryOnly,
            MatchNestingOptionEnum.CurrentFolderAndChildFolders => SearchOption
                .AllDirectories,
            _ => throw new UnexpectedSwitchValueException(
                criteria.NestingOption)
        };
    }

    private void PopulateResults(PhysicalStorageProvider storageProvider, IFindCriteria criteria, string fullPath)
    {
        var entry = File.Exists(fullPath)
            ? new PhysicalFileEntry(StorageProvider, new FileInfo(fullPath))
            : (IEntry)new PhysicalFolderEntry(StorageProvider, new DirectoryInfo(fullPath));
        if (criteria.IsMatch(entry))
            AllEntries.Add(entry);
    }
}
