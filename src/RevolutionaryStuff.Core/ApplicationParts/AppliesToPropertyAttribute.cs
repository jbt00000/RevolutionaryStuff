using System;
using System.Collections.Generic;
using System.Reflection;

namespace RevolutionaryStuff.Core.ApplicationParts
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public abstract class AppliesToPropertyAttribute : Attribute
    {
        protected readonly HashSet<string> PropertyNames = new HashSet<string>();

        public AppliesToPropertyAttribute(params string[] propertyNames)
        {
            if (propertyNames != null)
            {
                foreach (var pn in propertyNames) PropertyNames.Add(pn);
            }
        }

        protected static bool IsApplied<TAttr>(PropertyInfo pi) where TAttr : AppliesToPropertyAttribute
        {
            if (pi.GetCustomAttribute<TAttr>() != null) return true;
            var a = pi.DeclaringType.GetCustomAttribute<TAttr>();
            return a != null && a.PropertyNames.Contains(pi.Name);
        }
    }
}
