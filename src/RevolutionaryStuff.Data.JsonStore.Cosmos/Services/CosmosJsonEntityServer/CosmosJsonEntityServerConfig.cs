using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

public class CosmosJsonEntityServerConfig
{
    public const string ConfigSectionName = "CosmosJsonEntityServerConfig";

    public bool AuthenticateWithWithDefaultAzureCredentials { get; set; } = true;

    public ConnectionMode ConnectionMode { get; set; } = ConnectionMode.Direct;

    public string ApplicationName { get; set; }

    public RetryInfo PreconditionFailedRetryInfo { get; set; } = new(TimeSpan.FromMilliseconds(150), 7);
    public class RetryInfo
    {
        public TimeSpan DelayBetweenRetries { get; init; }
        public int MaxRetries { get; init; }
        public override string ToString()
            => $"delay={DelayBetweenRetries}, maxRetries={MaxRetries}";

        public RetryInfo()
        { }

        public RetryInfo(TimeSpan delayBetweenRetries, int maxRetries)
        {
            DelayBetweenRetries = delayBetweenRetries;
            MaxRetries = maxRetries;
        }
    }

    public bool EnableAnalytics { get; set; }

    public Dictionary<string, ContainerConfig> ContainerConfigByContainerId { get; set; } = new();

    public class ContainerConfig
    {
        public Dictionary<string, string> Settings { get; set; } = new();

        // When Settings was Dictionary<string, object>, booleans would come into .net as strings, so stop pretending, and make them strings.
        public bool GetSettingBool(string key, bool defaultValue = false)
            => Parse.ParseBool(Settings.GetValueOrDefault(key), defaultValue);
    }

    public class RequestChargeLogging
    {
        public CosmosOperationEnum Operation { get; set; }
        public double RequestCharge { get; set; } = 1;
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }

    public List<RequestChargeLogging> RequestChargeLoggings { get; set; } = new();
}
