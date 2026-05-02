using System.Text.Json;

namespace RevolutionaryStuff.Data.JsonStore.ChangeDataCapture;

/// <summary>
/// Represents a single document-level change event from any CDC-capable data store.
/// </summary>
public interface IChangeDataCaptureJsonEntityEvent
{
    /// <summary>The store-specific data type discriminator for the changed document (the "_jet" property value).</summary>
    string DataType { get; }

    /// <summary>The primary key of the changed document.</summary>
    string DocumentId { get; }

    /// <summary>The full changed document as a JsonElement.</summary>
    JsonElement DocumentElement { get; }

    /// <summary>Arbitrary properties extracted from the document or transport envelope.</summary>
    IDictionary<string, object> Properties { get; }

    /// <summary>The type of change event.</summary>
    ChangeDataCaptureEventTypeEnum ChangeDataCaptureEventType { get;  }
}

