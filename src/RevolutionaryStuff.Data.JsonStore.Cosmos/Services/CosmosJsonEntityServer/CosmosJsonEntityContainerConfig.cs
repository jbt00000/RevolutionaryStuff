namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

public class CosmosJsonEntityContainerConfig
{
    public const string ConfigSectionName = "CosmosJsonEntityContainerConfig";
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
#if DEBUG
        = true;
#else
            = false;
#endif
}
