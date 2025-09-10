namespace RevolutionaryStuff.Crm.MondayCom;

public interface IMondayComApi
{
    Task<MondayBoardAndItemId> CreateItemAsync(string boardId, string itemName, IDictionary<string, object?>? columnNameValues, string? groupName = null);
    Task<MondayBoardAndItemId> FindBoardItemAsync(string boardId, string fieldName, string? fieldVal);
    Task<IList<MondayBoardAndItemId>> FindItemsByFieldAsync(string boardId, string columnNameOrId, string? value, int? maxItems = null);
    async Task<MondayBoardAndItemId?> FindItemByFieldAsync(string boardId, string columnNameOrId, string? value)
    {
        var items = await FindItemsByFieldAsync(boardId, columnNameOrId, value, 1);
        return items.NullSafeEnumerable().FirstOrDefault();
    }
    Task<MondayBoard?> GetBoardByNameAsync(string name);
    Task<IList<MondayBoard>> GetBoardsAsync(IList<string>? boardIds = null);
}
