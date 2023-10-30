using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace RevolutionaryStuff.Core;

/// <summary>
/// Yes, is one of the rare classes that is setup to support Newtonsoft SystemTextJson and DataContract serialization
/// </summary>
[DataContract]
public class ExceptionError
{
    [DataMember(Name = "errorType")]
    [JsonProperty("errorType")]
    [JsonPropertyName("errorType")]
    public string ErrorType { get; set; }

    [DataMember(Name = "errorMessage")]
    [JsonProperty("errorMessage")]
    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; }

    [DataMember(Name = "errorCode")]
    [JsonProperty("errorCode")]
    [JsonPropertyName("errorCode")]
    public object ErrorCode { get; set; }

    [DataMember(Name = "innerErrors")]
    [JsonProperty("innerErrors")]
    [JsonPropertyName("innerErrors")]
    public IList<ExceptionError> InnerErrors { get; set; }

    [DataMember(Name = "errorStackTrace")]
    [JsonProperty("errorStackTrace")]
    [JsonPropertyName("errorStackTrace")]
    public string ErrorStackTrace { get; set; }

    public ExceptionError()
    { }

    public ExceptionError(Exception ex)
    {
        if (ex == null)
        {
            ErrorMessage = "<null>";
            return;
        }
        ErrorType = ex.GetType().Name;
        ErrorMessage = ex.Message;
        ErrorCode = BaseCodedException.GetCode(ex);
        ErrorStackTrace = ex.StackTrace;
        var kids = new List<ExceptionError>();
        if (ex.InnerException != null)
        {
            kids.Add(new ExceptionError(ex.InnerException));
        }
        if (ex is AggregateException)
        {
            ((AggregateException)ex).InnerExceptions.ForEach(iex => kids.Add(new ExceptionError(iex)));
        }
        if (kids.Count > 0)
        {
            InnerErrors = kids;
        }
    }

    public static implicit operator ExceptionError(Exception ex)
        => new(ex);

    public string ToJson()
        => JsonHelpers.ToJson(this);

    public static ExceptionError CreateFromJson(string json)
        => JsonHelpers.FromJson<ExceptionError>(json);
}
