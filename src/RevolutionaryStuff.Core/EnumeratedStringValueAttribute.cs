using System;
using System.Linq;

namespace RevolutionaryStuff.Core
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class EnumeratedStringValueAttribute : Attribute
    {
        public string Val { get; }

        public string Group { get; }

        public override string ToString() => $"{GetType().Name} val={Val} group={Group}";

        public EnumeratedStringValueAttribute(string val, string group=null)
        {
            Val = val;
            Group = group;
        }

        public static string GetValue(Enum e, string group = null) => e.GetCustomAttributes<EnumeratedStringValueAttribute>().FirstOrDefault(a => a.Group == group || group == null)?.Val;
    }
}
