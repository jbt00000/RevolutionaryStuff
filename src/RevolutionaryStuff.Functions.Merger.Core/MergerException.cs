using System;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Mergers;

namespace RevolutionaryStuff.Functions.Mergers
{
    public class MergerException : CodedException<MergerExceptionCodes>
    {
        public MergerException(MergerExceptionCodes code)
            : base(code)
        { }

        public MergerException(MergerExceptionCodes code, string message)
            : base(code, message)
        { }

        public MergerException(MergerExceptionCodes code, Exception inner)
            : base(code, inner)
        { }
    }
}
