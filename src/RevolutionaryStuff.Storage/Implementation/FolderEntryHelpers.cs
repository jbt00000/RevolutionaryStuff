namespace RevolutionaryStuff.Storage.Implementation;

public static class FolderEntryHelpers
{
    public static async Task<IList<IEntry>> GetAllEntriesAsync(this IFolderEntry folder, bool recurse = false)
    {
        var items = new List<IEntry>();
        var res = await folder.FindAsync(recurse ? FindCriteria.AllItemsAllFolderFindCriteria : FindCriteria.AllItemsCurrentFolderFindCriteria);
Again:
        if (res != null && res.Entries != null && res.Entries.Count > 0)
        {
            items.AddRange(res.Entries);
            res = await res.NextAsync();
            goto Again;
        }
        return items;
    }
}
