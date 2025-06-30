using static RevolutionaryStuff.Crm.Implementation.Crm;

namespace RevolutionaryStuff.Crm.Implementation;

public abstract class Crm(CrmConstructorArgs MyConstructorArgs, ILogger logger)
    : BaseLoggingDisposable(logger), ICrm
{
    private static readonly FindCrmItemSettings DefaultFindCrmItemSettings = new();
    private static readonly CreateCrmItemSettings DefaultCreateCrmItemSettings = new();
    private static readonly FindOrCreateCrmItemSettings DefaultFindOrCreateCrmItemSettings = new()
    {
        CreateCrmItemSettings = DefaultCreateCrmItemSettings,
        FindCrmItemSettings = DefaultFindCrmItemSettings
    };

    public sealed record CrmConstructorArgs()
    { }

    private ICrm I => this;


    Task<FindCrmItemResult> ICrm.FindRecordByFieldValueAsync(string tableName, string fieldName, object fieldVal, FindCrmItemSettings? settings)
    {
        Requires.Text(tableName);
        Requires.Text(fieldName);
        return OnFindRecordByFieldValueAsync(tableName, fieldName, fieldVal, settings ?? DefaultFindCrmItemSettings);
    }

    protected abstract Task<FindCrmItemResult> OnFindRecordByFieldValueAsync(string tableName, string fieldName, object fieldVal, FindCrmItemSettings settings);


    Task<CreateCrmItemResult> ICrm.CreateContactAsync(ICrmContact contact, CreateCrmItemSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(contact);
        return OnCreateContactAsync(contact, settings ?? DefaultCreateCrmItemSettings);
    }

    protected abstract Task<CreateCrmItemResult> OnCreateContactAsync(ICrmContact contact, CreateCrmItemSettings settings);

    Task<CreateCrmItemResult> ICrm.CreateLeadAsync(ICrmContact contact, CreateCrmItemSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(contact);
        return OnCreateLeadAsync(contact, settings ?? DefaultCreateCrmItemSettings);
    }

    protected abstract Task<CreateCrmItemResult> OnCreateLeadAsync(ICrmContact contact, CreateCrmItemSettings settings);

    private CrmContactFieldEnum GetCrmContactFieldEnum(CrmLeadContactJointFieldEnum e)
        => Parse.ParseEnumWithEnumMemberValues<CrmContactFieldEnum>(e.EnumWithEnumMemberValuesToString());

    private CrmLeadFieldEnum GetCrmLeadFieldEnum(CrmLeadContactJointFieldEnum e)
        => Parse.ParseEnumWithEnumMemberValues<CrmLeadFieldEnum>(e.EnumWithEnumMemberValuesToString());

    async Task<FindCrmItemResult> ICrm.FindContactOrLeadByFieldValueAsync(CrmLeadContactJointFieldEnum fieldName, object fieldVal, FindCrmItemSettings? settings)
    {
        var tLead = I.FindLeadByFieldValueAsync(GetCrmLeadFieldEnum(fieldName), fieldVal, settings);
        var tContact = I.FindContactByFieldValueAsync(GetCrmContactFieldEnum(fieldName), fieldVal, settings);
        await Task.WhenAll(tLead, tContact);
        return tContact.Result ?? tLead.Result;
    }


    async Task<FindOrCreateCrmItemResult> ICrm.FindCreateContactOrLeadOnMissingCreateLeadAsync(CrmLeadContactJointFieldEnum fieldName, object fieldVal, ICrmContact contact, FindOrCreateCrmItemSettings? settings)
    {
        var findSettings = settings?.FindCrmItemSettings ?? DefaultFindOrCreateCrmItemSettings.FindCrmItemSettings;
        var createSettings = settings?.CreateCrmItemSettings ?? DefaultFindOrCreateCrmItemSettings.CreateCrmItemSettings;
        var findResult = await I.FindContactOrLeadByFieldValueAsync(fieldName, fieldVal, findSettings);
        if (findResult.Success)
            return FindOrCreateCrmItemResult.CreateFromResult(findResult);
        else
        {
            var createResult = await I.CreateLeadAsync(contact, createSettings);
            return FindOrCreateCrmItemResult.CreateFromResult(createResult);
        }
    }

    protected abstract Task OnCheckHasTableAsync(string tableName);

    protected Task CheckHasTableAsync(string tableName)
    {
        Requires.Text(tableName);
        return OnCheckHasTableAsync(tableName);
    }

    async Task<CreateCrmItemResult> ICrm.CreateItemAsync(string tableName, string name, IDictionary<string, object> fieldByName, CreateCrmItemSettings? settings)
    {
        await CheckHasTableAsync(tableName);
        return await OnCreateItemAsync(tableName, name, fieldByName, settings ?? DefaultCreateCrmItemSettings);
    }

    protected abstract Task<CreateCrmItemResult> OnCreateItemAsync(string tableName, string name, IDictionary<string, object> fieldByName, CreateCrmItemSettings settings);
}
