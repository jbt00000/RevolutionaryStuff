using Microsoft.Extensions.Options;

namespace RevolutionaryStuff.Core.Services.Correlation;

internal class CorrelationIdFactory : ICorrelationIdFactory
{
    private readonly IOptions<Config> ConfigOptions;

    public class Config
    {
        public const string ConfigSectionName = Stuff.ConfigSectionNamePrefix + "CorrelationIdFactoryConfig";

        public string StringFormat { get; set; } = "rsllc-xcid-{0}";
    }

    public CorrelationIdFactory(IOptions<Config> configOptions)
    {
        ArgumentNullException.ThrowIfNull(configOptions);
        ConfigOptions = configOptions;
    }

    string ICorrelationIdFactory.Create()
        => string.Format(ConfigOptions.Value.StringFormat, Guid.NewGuid());
}
