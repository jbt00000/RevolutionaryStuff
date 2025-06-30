namespace RevolutionaryStuff.Crm;

public interface ICrm
{
    Task<FindCrmItemResult> FindRecordByFieldValueAsync(string tableName, string fieldName, object fieldVal, FindCrmItemSettings? settings = null);

    Task<FindCrmItemResult> FindContactByFieldValueAsync(CrmContactFieldEnum field, object val, FindCrmItemSettings? settings = null)
        => FindRecordByFieldValueAsync(CrmRecordTypeEnum.Contact.EnumWithEnumMemberValuesToString(), field.EnumWithEnumMemberValuesToString(), val);

    Task<FindCrmItemResult> FindContactByFieldValueAsync(string fieldName, object val, FindCrmItemSettings? settings = null)
        => FindRecordByFieldValueAsync(CrmRecordTypeEnum.Contact.EnumWithEnumMemberValuesToString(), fieldName, val, settings);

    Task<FindCrmItemResult> FindLeadByFieldValueAsync(CrmLeadFieldEnum field, object val, FindCrmItemSettings? settings = null)
        => FindRecordByFieldValueAsync(CrmRecordTypeEnum.Lead.EnumWithEnumMemberValuesToString(), field.EnumWithEnumMemberValuesToString(), val, settings);

    Task<FindCrmItemResult> FindLeadByFieldValueAsync(string fieldName, object val, FindCrmItemSettings? settings = null)
        => FindRecordByFieldValueAsync(CrmRecordTypeEnum.Lead.EnumWithEnumMemberValuesToString(), fieldName, val, settings);

    Task<CreateCrmItemResult> CreateContactAsync(ICrmContact contact, CreateCrmItemSettings? settings = null);

    Task<CreateCrmItemResult> CreateLeadAsync(ICrmContact contact, CreateCrmItemSettings? settings = null);

    Task<FindCrmItemResult> FindContactOrLeadByFieldValueAsync(CrmLeadContactJointFieldEnum fieldName, object fieldVal, FindCrmItemSettings? settings = null);

    Task<FindOrCreateCrmItemResult> FindCreateContactOrLeadOnMissingCreateLeadAsync(CrmLeadContactJointFieldEnum fieldName, object fieldVal, ICrmContact contact, FindOrCreateCrmItemSettings? settings = null);

    Task<CreateCrmItemResult> CreateItemAsync(string tableName, string name, IDictionary<string, object> fieldByName, CreateCrmItemSettings? settings = null);
}
