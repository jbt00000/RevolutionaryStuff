using System;
using System.Reflection;
using System.Text.RegularExpressions;
using RevolutionaryStuff.Core.Collections;

namespace RevolutionaryStuff.Core
{
    /// <summary>
    /// Class to represent common Attribute values that are attached to an assembly typically from the AssemblyInfo.cs file
    /// </summary>
    public class AssemblyAttributeInfo
    {
        private static readonly Regex ParseVersionExpr = new Regex(@"Version=(\d+\.\d+\.\d+\.\d+)", RegexHelpers.IgnoreCaseSingleLineCompiled);

        public readonly Assembly Assembly;
        public readonly string FullName;
        public readonly string Title;
        public readonly string Description;
        public readonly string Configuration;
        public readonly string Company;
        public readonly string Product;
        public readonly string Copyright;
        public readonly string Trademark;
        public readonly string Culture;
        public readonly string Version;
        public readonly string FileVersion;
        public readonly MultipleValueDictionary<Type, Attribute> AttributesByType = new MultipleValueDictionary<Type, Attribute>();

        public AssemblyAttributeInfo(Assembly a)
        {
            Requires.NonNull(a, nameof(a));

            Assembly = a;
            FullName = a.FullName;

            foreach (Attribute attr in a.GetCustomAttributes())
            {
                var t = attr.GetType();
                AttributesByType.Add(t, attr);
                if (t == typeof(AssemblyTitleAttribute))
                {
                    Title = ((AssemblyTitleAttribute)attr).Title;
                }
                else if (t == typeof(AssemblyDescriptionAttribute))
                {
                    Description = ((AssemblyDescriptionAttribute)attr).Description;
                }
                else if (t == typeof(AssemblyConfigurationAttribute))
                {
                    Configuration = ((AssemblyConfigurationAttribute)attr).Configuration;
                }
                else if (t == typeof(AssemblyCompanyAttribute))
                {
                    Company = ((AssemblyCompanyAttribute)attr).Company;
                }
                else if (t == typeof(AssemblyProductAttribute))
                {
                    Product = ((AssemblyProductAttribute)attr).Product;
                }
                else if (t == typeof(AssemblyCopyrightAttribute))
                {
                    Copyright = ((AssemblyCopyrightAttribute)attr).Copyright;
                }
                else if (t == typeof(AssemblyTrademarkAttribute))
                {
                    Trademark = ((AssemblyTrademarkAttribute)attr).Trademark;
                }
                else if (t == typeof(AssemblyCultureAttribute))
                {
                    Culture = ((AssemblyCultureAttribute)attr).Culture;
                }
                else if (t == typeof(AssemblyVersionAttribute))
                {
                    Version = ((AssemblyVersionAttribute)attr).Version;
                }
                else if (t == typeof(AssemblyFileVersionAttribute))
                {
                    FileVersion = ((AssemblyFileVersionAttribute)attr).Version;
                }
            }
            if (Version == null)
            {
                var m = ParseVersionExpr.Match(a.FullName);
                if (m.Success)
                {
                    Version = m.Groups[1].Value;
                }
            }
            AttributesByType.MakeReadOnly();
        }
    }
}
