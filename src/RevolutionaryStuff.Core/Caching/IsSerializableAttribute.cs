using System;
using System.Collections.Generic;
using System.Text;

namespace RevolutionaryStuff.Core.Caching
{
    public class IsSerializableAttribute : Attribute
    {
        public bool IsSerializable { get; private set; }

        public IsSerializableAttribute(bool isSerializable)
        {
            IsSerializable = isSerializable;
        }
    }
}
