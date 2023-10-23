using System.IO;
using Microsoft.Extensions.Options;

namespace RevolutionaryStuff.Core.Services.TemporaryStreamFactory;

internal class RsllcTemporaryStreamFactory : ITemporaryStreamFactory
{
    private readonly IOptions<Config> ConfigOptions;

    public class Config
    {
        public const string ConfigSectionName = "TemporaryStreamFactory";

        public int MemoryStreamExpectedCapacityLimit { get; set; } = 1024 * 32;

        public int FileBufferSize { get; set; } = 1024 * 16;
    }

    public RsllcTemporaryStreamFactory(IOptions<Config> configOptions)
    {
        ArgumentNullException.ThrowIfNull(configOptions);

        ConfigOptions = configOptions;
    }

    Stream ITemporaryStreamFactory.Create(long? capacity)
    {
        var config = ConfigOptions.Value;
        if (capacity.HasValue && capacity.Value < int.MaxValue)
        {
            if (capacity.GetValueOrDefault(int.MaxValue) < config.MemoryStreamExpectedCapacityLimit)
            {
                return new MemoryStream(Convert.ToInt32(capacity.Value));
            }
        }
        var fn = Path.GetTempFileName();
        return File.Create(fn, config.FileBufferSize, FileOptions.DeleteOnClose);
    }
}
