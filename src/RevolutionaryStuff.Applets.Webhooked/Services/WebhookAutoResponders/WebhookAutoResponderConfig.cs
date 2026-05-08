using System.Collections.Generic;
using System.Net;

namespace RevolutionaryStuff.Applets.Webhooked;

public class WebhookAutoResponderConfig
{
    public const string ConfigSectionName = "WebhookResponder";

    public string? BaseFolderName { get; set; }

    public bool RespondWithDetailedErrors { get; set; }

    public Dictionary<string, WebhookServiceConfig>? Services { get; set; }

    public class WebhookAuthenticationConfig
    {
        public string? BasicAuthUser { get; set; }
        public string? BasicAuthPass { get; set; }
        public string? QueryStringParameterName { get; set; }
        public string? QueryStringParameterValue { get; set; }
        public string? AcceptableContentType { get; set; }
    }

    public class WebhookServiceConfig
    {
        /// <summary>
        /// When true, this endpoint should be enabled, else false
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// When true, store the request to the blob storage, also requires StorageFolderName to be set
        /// </summary>
        public bool StoreRequest { get; set; } = true;

        /// <summary>
        /// Name of the storage folder to use when storing the request
        /// </summary>
        public string? StorageFolderName { get; set; }

        /// <summary>
        /// The http status code to respond with if this was a successful request
        /// </summary>
        public HttpStatusCode SuccessCode { get; set; } = HttpStatusCode.OK;

        /// <summary>
        /// Service Bus topic in which to send the payload
        /// </summary>
        public string? Topic { get; set; }

        public WebhookAuthenticationConfig? AuthenticationConfig { get; set; }

        public string? WebRoute { get; set; }
        public IList<string>? AllowedMethods { get; set; } = [WebHelpers.Methods.Post];
    }
}
