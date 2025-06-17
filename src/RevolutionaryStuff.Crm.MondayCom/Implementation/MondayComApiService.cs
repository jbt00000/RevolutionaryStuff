using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core.Services.Http;
using RevolutionaryStuff.Core.Threading;

namespace RevolutionaryStuff.Crm.MondayCom.Implementation;

/// <remarks>
/// https://developer.monday.com/api-reference/reference/items
/// https://developer.monday.com/api-reference/reference/column-types-reference
/// </remarks>
internal class MondayComApiService : BaseLoggingDisposable, IMondayComApi
{

    public class Config
    {
        public const string ConfigSectionName = "MondayComApi";

        public string ApiToken { get; set; }

        public string BaseUrl { get; set; } = "https://api.monday.com/v2/";
    }

    private readonly IMondayComApi I;
    private readonly IOptions<Config> ConfigOptions;
    private readonly IHttpMessageSender HttpMessageSender;
    private readonly AsyncLocker GetBoardsLocker = new();

    public MondayComApiService(IOptions<Config> configOptions, IHttpMessageSender httpMessageSender, ILogger<MondayComApiService> logger)
        : base(logger)
    {
        I = this;
        ConfigOptions = configOptions;
        HttpMessageSender = httpMessageSender;
    }

    protected override void OnDispose(bool disposing)
    {
        base.OnDispose(disposing);
        if (disposing)
            GetBoardsLocker.Dispose();
    }

    private async Task<string> SendGraphQLAsync(string query, object variables = null)
    {
        var config = ConfigOptions.Value;
        var requestBody = JsonSerializer.Serialize(new { query, variables });
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri(config.BaseUrl))
        {
            Content = new StringContent(requestBody, Encoding.UTF8, MimeType.Application.Json.PrimaryContentType)
        };
        request.Headers.Add("Authorization", config.ApiToken);

        var response = await HttpMessageSender.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private IList<MondayBoard> Boards;

    async Task<MondayBoard> IMondayComApi.GetBoardByNameAsync(string name)
    {
        var boards = await I.GetBoardsAsync();
        return boards.FirstOrDefault(b => b.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    async Task<IList<MondayBoard>> IMondayComApi.GetBoardsAsync(IList<string> boardIds)
    {
        if (Boards == null)
        {
            await GetBoardsLocker.GoAsync(async () =>
            {
                if (Boards != null) return;
                // Query all boards (no ids argument)
                var query = @"
        query {
            boards (limit:500) {
                id
                name
                description
                state
                type
                updated_at
                url
                board_kind
                top_group {
                    id
                    title
                }
                groups {
                    id
                    title
                }
                owner {
                    id
                    name
                }
                columns {
                    id
                    title
                    type
                    description
                    settings_str
                    archived
                }
            }
        }";

                var json = await SendGraphQLAsync(query, null);

                using var doc = JsonDocument.Parse(json);
                var boardsElement = doc.RootElement.GetProperty("data").GetProperty("boards");
                Boards = JsonSerializer.Deserialize<IList<MondayBoard>>(boardsElement.GetRawText());
            });
        }
        return Boards.NullSafeEnumerable().Where(z => boardIds == null || boardIds.Contains(z.Id)).ToList().AsReadOnly();
    }

    // Find item by column value
    async Task<IList<MondayBoardAndItemId>> IMondayComApi.FindItemsByFieldAsync(string boardId, string columnNameOrId, string value, int? maxItems)
    {
        Requires.Text(columnNameOrId);

        var board = (await I.GetBoardsAsync([boardId])).NullSafeEnumerable().FirstOrDefault();
        ItemNotFoundException.ThrowIfNull(board, boardId);

        var column = board.GetColumnByName(columnNameOrId);

        var query = @"
        query ($limit: Int!, $boardId: ID!, $columnId: String!, $value: String!) {
          items_page_by_column_values (limit: $limit, board_id: $boardId, columns: [{column_id: $columnId, column_values: [$value]}]) {
            cursor
            items {
              id
              name
            }
          }
        }";

        var variables = new { limit = maxItems ?? 100, boardId = long.Parse(boardId), columnId = column.Id, value };
        var json = await SendGraphQLAsync(query, variables);

        // Parse the JSON response
        using var doc = JsonDocument.Parse(json);
        var results = new List<MondayBoardAndItemId>();

        // Navigate to the items array
        if (doc.RootElement.TryGetProperty("data", out var dataElement) &&
            dataElement.TryGetProperty("items_page_by_column_values", out var pageElement) &&
            pageElement.TryGetProperty("items", out var itemsElement))
        {
            // Process each item in the array
            foreach (var item in itemsElement.EnumerateArray())
            {
                var itemId = item.GetProperty("id").GetString();
                var itemName = item.GetProperty("name").GetString();
                results.Add(new MondayBoardAndItemId(board, itemId, itemName));

                // Respect the maxItems limit
                if (maxItems.HasValue && results.Count >= maxItems.Value)
                    break;
            }
        }

        return results;
    }

    async Task<MondayBoardAndItemId> IMondayComApi.CreateItemAsync(
        string boardId,
        string itemName,
        IDictionary<string, object?>? columnNameValues)
    {
        // Get the board and its columns
        var board = (await I.GetBoardsAsync([boardId])).NullSafeEnumerable().FirstOrDefault();
        ItemNotFoundException.ThrowIfNull(board, boardId);

        // Map column names to IDs
        Dictionary<string, object?> columnValues = [];
        foreach (var kvp in columnNameValues ?? new Dictionary<string, object?>())
        {
            var column = board!.GetColumnByName(kvp.Key);
            if (column == null)
                throw new ArgumentException($"Column with name '{kvp.Key}' not found on board '{board.Name}'.");
            var val = kvp.Value;
            switch (column.ColumnType)
            {
                case ColumnTypeEnum.Date:
                    if (val is DateTimeOffset dto)
                    {
                        dto = dto.ToUniversalTime();
                        val = new
                        {
                            date = dto.Date.ToYYYY_MM_DD(),
                            time = dto.ToMilitaryTime()
                        };
                    }
                    else if (val is DateTime dt)
                    {
                        val = new
                        {
                            date = dt.ToYYYY_MM_DD(),
                            time = dt.ToMilitaryTime()
                        };
                    }
                    else
                    {
                        val = val is DateOnly dateOnly
                            ? (new
                            {
                                date = dateOnly.ToDateTime(TimeOnly.MinValue).ToYYYY_MM_DD(),
                            })
                            : null;
                    }
                    break;
                case ColumnTypeEnum.BoardRelation:
                    if (val is string sval)
                    {
                        val = new
                        {
                            item_ids = new string[] { sval }
                        };
                    }
                    else
                    {
                        val = val is IEnumerable<string> svals
                            ? (new
                            {
                                item_ids = svals.ToList()
                            })
                            : null;
                    }
                    break;
                case ColumnTypeEnum.Location:
                    if (val is IMailingAddress physicalAddress)
                    {
                        var coordinates = physicalAddress as IGeographicCoordinates;
                        val = new
                        {
                            address = physicalAddress.CreateFreeform(),
                            lng = $"{(coordinates?.Longitude).GetValueOrDefault()}",
                            lat = $"{(coordinates?.Latitude).GetValueOrDefault()}"
                        };
                    }
                    else
                    {
                        val = null;
                    }
                    break;
                case ColumnTypeEnum.Link:
                    val = val is Uri || val is string
                        ? (new
                        {
                            url = val.ToString(),
                            text = val.ToString()
                        })
                        : null;
                    break;
                case ColumnTypeEnum.Status:
                    var cleanStatus = column.GetLookupLabel(val?.ToString());
                    val = cleanStatus != null
                        ? (new
                        {
                            label = cleanStatus,
                        })
                        : null;
                    break;
                case ColumnTypeEnum.Dropdown:
                    var cleanLabel = column.GetLookupLabel(val?.ToString());
                    val = cleanLabel != null
                        ? (new
                        {
                            labels = new string[] { cleanLabel },
                        })
                        : null;
                    break;
                case ColumnTypeEnum.Unsupported:
                    throw new NotSupportedException($"Column type '{column.RawColumnType}' is not supported.");
                default:
                    break;
            }
            if (val == null) continue;
            columnValues[column.Id] = val;
        }

        var query = @"
        mutation ($boardId: ID!, $itemName: String!, $groupId: String, $columnValues: JSON) {
            create_item(board_id: $boardId, item_name: $itemName, group_id: $groupId, column_values: $columnValues, create_labels_if_missing :true) {
                id
                name
            }
        }";
        var variables = new { boardId = long.Parse(boardId), itemName, groupId = board.TopGroup.Id, columnValues = JsonSerializer.Serialize(columnValues) };
        var json = await SendGraphQLAsync(query, variables);
        var createItemResponse = JsonSerializer.Deserialize<MondayCreateItemResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return new(board, createItemResponse?.Data?.CreateItem?.Id, createItemResponse?.Data?.CreateItem?.Name);
    }


    private class MondayCreateItemResponse
    {
        [JsonPropertyName("data")]
        public MondayCreateItemData Data { get; set; }

        [JsonPropertyName("extensions")]
        public MondayCreateItemExtensions Extensions { get; set; }
        public class MondayCreateItemData
        {
            [JsonPropertyName("create_item")]
            public MondayCreatedItem CreateItem { get; set; }
        }

        public class MondayCreatedItem
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }

        public class MondayCreateItemExtensions
        {
            [JsonPropertyName("request_id")]
            public string? RequestId { get; set; }
        }
    }

    async Task<MondayBoardAndItemId> IMondayComApi.FindBoardItemAsync(string boardId, string fieldName, string fieldVal)
    {
        var board = (await I.GetBoardsAsync([boardId])).NullSafeEnumerable().FirstOrDefault();
        ItemNotFoundException.ThrowIfNull(board, boardId);

        var col = board.GetColumnByName(fieldName);
        return col != null ? await I.FindItemByFieldAsync(board.Id.ToString(), col.Id, fieldVal) : null;
    }
}
