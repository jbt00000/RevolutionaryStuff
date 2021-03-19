using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RevolutionaryStuff.Core.ApplicationParts
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class JsonConverterTypeAttribute : Attribute
    {
        public Type JsonConverterType;

        public static readonly IList<JsonConverter> JsonConverters = new List<JsonConverter>();

        public JsonConverterTypeAttribute(Type t)
        {
            Requires.IsType(t, typeof(JsonConverter));

            JsonConverterType = t;
            JsonConverters.Add((JsonConverter)t.Construct());
        }
    }

}
