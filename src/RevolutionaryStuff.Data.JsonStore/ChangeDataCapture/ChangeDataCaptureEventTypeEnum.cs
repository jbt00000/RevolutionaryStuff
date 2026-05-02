namespace RevolutionaryStuff.Data.JsonStore.ChangeDataCapture;

public enum ChangeDataCaptureEventTypeEnum
{
    Unknown = 0,
    /// <summary>The document was either inserted or updated.</summary>
    Changed, 
    /// <summary>The document was inserted.</summary>
    Inserted,
    /// <summary>The document was updated.</summary>
    Updated,
    /// <summary>The document was deleted.</summary>
    Deleted,
}
