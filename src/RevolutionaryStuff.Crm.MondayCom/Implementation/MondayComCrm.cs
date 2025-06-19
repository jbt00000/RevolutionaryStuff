using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core.Services.DependencyInjection;

namespace RevolutionaryStuff.Crm.MondayCom.Implementation;

[NamedService(NamedServiceName)]
internal class MondayComCrm(IMondayComApi Api, IOptions<MondayComCrm.Config> ConfigOptions, Crm.Implementation.Crm.CrmConstructorArgs baseConstructorArgs, ILogger<MondayComCrm> logger)
    : Crm.Implementation.Crm(baseConstructorArgs, logger), IMondayComCrm
{
    public const string NamedServiceName = "MondayComCrm";
    public class Config
    {
        public const string ConfigSectionName = "MondayComCrm";

        public Dictionary<string, BoardInfo> RecordMap { get; set; } = new()
        {
            { CrmRecordTypeEnum.Contact.EnumWithEnumMemberValuesToString(), new("contacts", new(){
                { CrmContactFieldEnum.Name.EnumWithEnumMemberValuesToString(), "Contact" },
                { CrmContactFieldEnum.Email.EnumWithEnumMemberValuesToString(), "Email" },
                { CrmContactFieldEnum.Phone.EnumWithEnumMemberValuesToString(), "Phone" },
                { CrmContactFieldEnum.Address.EnumWithEnumMemberValuesToString(), "Address" },
            }) },
            { CrmRecordTypeEnum.Lead.EnumWithEnumMemberValuesToString(), new("leads", new(){
                { CrmLeadFieldEnum.Name.EnumWithEnumMemberValuesToString(), "Lead" },
                { CrmLeadFieldEnum.Email.EnumWithEnumMemberValuesToString(), "Email" },
                { CrmLeadFieldEnum.Phone.EnumWithEnumMemberValuesToString(), "Phone" },
                { CrmLeadFieldEnum.Address.EnumWithEnumMemberValuesToString(), "Address" },
            }) },
        };

        public class BoardInfo
        {
            public string MondayBoardName { get; set; }
            public Dictionary<string, string> FieldMap { get; set; }
            public BoardInfo() { }

            public BoardInfo(string boardName, Dictionary<string, string> fieldMap)
            {
                MondayBoardName = boardName;
                FieldMap = fieldMap;
            }

            public string GetMondayFieldName(string fieldName)
            {
                return FieldMap == null ? fieldName : FieldMap.TryGetValue(fieldName, out var mondayFieldName) ? mondayFieldName : fieldName;
            }
        }
    }

    protected static void RequiresBoardName(string boardName)
        => Requires.Text(boardName);

    protected static void RequiresFieldName(string fieldName)
        => Requires.Text(fieldName);

    private record BoardMap(MondayBoard Board, Config.BoardInfo Info)
    {
        public IDictionary<string, object> MapFieldVals(IDictionary<string, object> fieldVals, HashSet<string> keysToIgnore = null, bool skipNull = true)
        {
            var result = new Dictionary<string, object>();
            foreach (var kv in fieldVals)
            {
                if (skipNull && kv.Value == null) continue;
                if (keysToIgnore?.Contains(kv.Key) == true) continue;
                var fieldName = GetNormalizedFieldName(kv.Key);
                if (fieldName != null)
                    result[fieldName] = kv.Value;
            }
            return result;
        }

        public string GetNormalizedFieldName(string fieldName)
            => Info?.GetMondayFieldName(fieldName) ?? Board?.GetColumnByName(fieldName)?.Title;
    }

    private Task<BoardMap?> GetMondayBoardAsync(CrmRecordTypeEnum crmRecordType)
        => GetMondayBoardAsync(crmRecordType.EnumWithEnumMemberValuesToString());

    private async Task<BoardMap?> GetMondayBoardAsync(string crmRecordType)
    {
        var info = ConfigOptions.Value.RecordMap?.FindOrDefault(crmRecordType);
        var board = await Api.GetBoardByNameAsync(info?.MondayBoardName ?? crmRecordType);
        return board == null ? null : new(board, info);
    }

    protected override async Task<FindCrmItemResult> OnFindRecordByFieldValueAsync(string tableName, string fieldName, object fieldVal, FindCrmItemSettings settings)
    {
        RequiresBoardName(tableName);
        RequiresFieldName(fieldName);

        var m = await GetMondayBoardAsync(tableName);
        ItemNotFoundException.ThrowIfNull(m, tableName);

        var res = await Api.FindItemByFieldAsync(m.Board.Id, m.GetNormalizedFieldName(fieldName), fieldVal?.ToString());
        return new FindCrmItemResult(res?.ItemId);
    }

    private static readonly HashSet<string> ContactFieldsToIgnoreOnCreate = new(Comparers.CaseInsensitiveStringComparer)
    {
        CrmContactFieldEnum.Name.EnumWithEnumMemberValuesToString(),
    };

    protected override async Task<CreateCrmItemResult> OnCreateContactAsync(ICrmContact contact, CreateCrmItemSettings settings)
    {
        var m = await GetMondayBoardAsync(CrmRecordTypeEnum.Contact);
        var res = await Api.CreateItemAsync(m.Board.Id, contact.Name, m.MapFieldVals(contact.ToDictionary(), ContactFieldsToIgnoreOnCreate), settings?.GroupName);
        return new CreateCrmItemResult(res.ItemId);
    }

    protected override async Task<CreateCrmItemResult> OnCreateLeadAsync(ICrmContact contact, CreateCrmItemSettings settings)
    {
        var m = await GetMondayBoardAsync(CrmRecordTypeEnum.Lead);
        var res = await Api.CreateItemAsync(m.Board.Id, contact.Name, m.MapFieldVals(contact.ToDictionary(), [CrmLeadFieldEnum.Name.EnumWithEnumMemberValuesToString()]), settings?.GroupName);
        return new CreateCrmItemResult(res.ItemId);
    }

    protected override async Task OnCheckHasTableAsync(string tableName)
    {
        var b = await GetMondayBoardAsync(tableName);
        ItemNotFoundException.ThrowIfNull(b, tableName);
    }

    protected override async Task<CreateCrmItemResult> OnCreateItemAsync(string tableName, string name, IDictionary<string, object> fieldByName, CreateCrmItemSettings settings)
    {
        var m = await GetMondayBoardAsync(tableName);
        ItemNotFoundException.ThrowIfNull(m, tableName);
        var res = await Api.CreateItemAsync(m.Board.Id, name, m.MapFieldVals(fieldByName, null), settings?.GroupName);
        return new CreateCrmItemResult(res?.ItemId);
    }
}
