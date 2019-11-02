using System;
using Microsoft.Extensions.Options;

namespace RevolutionaryStuff.Core.ApplicationParts
{
    internal class HardcodedOptionsMonitor<TOptions> : IOptionsMonitor<TOptions>
    {
        private readonly TOptions Options;

        public HardcodedOptionsMonitor(TOptions options)
        {
            Options = options;
        }

        TOptions IOptionsMonitor<TOptions>.CurrentValue => Options;

        TOptions IOptionsMonitor<TOptions>.Get(string name)
            => Options;

        IDisposable IOptionsMonitor<TOptions>.OnChange(Action<TOptions, string> listener)
            => new DoNothing();

        private class DoNothing : BaseDisposable { }
    }
}
