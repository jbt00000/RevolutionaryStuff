using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RevolutionaryStuff.Core.ApplicationParts
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class CommandLineSwitchAttribute : Attribute
    {
        public static class CommonArgNames
        {
            public const string AuthSection = "authSection";
            public const string Mode = "m";
            public const string InFile = "i";
            public const string OutFile = "o";
            public const string Domain = "d";
            public const string User = "u";
            public const string Password = "p";
            public const string Url = "url";
            public const string UserAgent = "ua";
            public const string Threads = "threads";
        }

        public string Key;
        public string AppSettingsName;
        public bool UseMemberName;
        public bool Mandatory;
        public string[] Modes;
        public string Mode;

        public string[] Names;
        public string Description;
        public CommandLineSwitchAttributeTranslators Translator;

        public bool IsValidForMode(string mode)
        {
            if (string.IsNullOrEmpty(mode)) return true;
            if (!string.IsNullOrEmpty(Mode))
            {
                if (Modes != null && Modes.Length > 0)
                {
                    throw new NotSupportedException("Cannot specify both Mode and Modes");
                }
                return 0 == StringHelpers.CompareIgnoreCase(Mode, mode);
            }
            if (Modes == null || Modes.Length == 0) return true;
            foreach (var m in this.Modes)
            {
                if (0 == StringHelpers.CompareIgnoreCase(mode, m)) return true;
            }
            return false;
        }

        public bool IsMandatoryForMode(string mode)
        {
            return this.Mandatory && IsValidForMode(mode);
        }

        #region Constructors

        public CommandLineSwitchAttribute(string name)
            : this(name, false)
        { }

        public CommandLineSwitchAttribute(string name, bool mandatory)
            : this(name, mandatory, null)
        { }

        public CommandLineSwitchAttribute(string name, string description)
            : this(name, false, description)
        { }

        public CommandLineSwitchAttribute(string name, bool mandatory, string description)
        {
            Requires.Text(name, "name");
            Names = new[] { name };
            Mandatory = mandatory;
            Description = description;
            Key = name;
        }

        #endregion

        public override string ToString()
        {
            return string.Format("CommandLineSwitch {0}", this.Names[0]);
        }

        public static IEnumerable<KeyValuePair<CommandLineSwitchAttribute, MemberInfo>> Find(Type t)
        {
            Requires.NonNull(t, nameof(t));

            var ti = t.GetTypeInfo();
            var ret = new List<KeyValuePair<CommandLineSwitchAttribute, MemberInfo>>();

            foreach (var pi in ti.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                var os = pi.GetCustomAttribute<CommandLineSwitchAttribute>();
                if (os != null)
                {
                    ret.Add(new KeyValuePair<CommandLineSwitchAttribute, MemberInfo>(os, pi));
                }
            }

            foreach (var fi in ti.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                var os = fi.GetCustomAttribute<CommandLineSwitchAttribute>();
                if (os != null)
                {
                    ret.Add(new KeyValuePair<CommandLineSwitchAttribute, MemberInfo>(os, fi));
                }
            }

            return ret.Where(z => z.Value.CanWrite());
        }

        private static string ToHelpString(Type t)
        {
            if (t.GetTypeInfo().IsEnum)
            {
                return "{" + Enum.GetNames(t).Format("|") + "}";
            }
            else
            {
                string name = t.FullName;
                if (name.StartsWith("System.") && name.IndexOf(".", 7) == -1)
                {
                    return t.Name;
                }
                return name;
            }
        }

        public static string SimpleUsageSwitchFormatter(KeyValuePair<CommandLineSwitchAttribute, MemberInfo> s, int pos)
        {
            var a = s.Key;
            var mi = s.Value;
            return string.Format(
                "\t-{0}:{1}:{2}{3}:{4}",
                a.Names.Format("|"),
                a.Mandatory ? "required" : "optional",
                ToHelpString(mi.GetUnderlyingType()),
                a.Translator == CommandLineSwitchAttributeTranslators.None ? "" : string.Format("({0})", a.Translator),
                a.Description
                );
        }

        public static string MandatesUsageSwitchFormatter(KeyValuePair<CommandLineSwitchAttribute, MemberInfo> s, int pos)
        {
            var a = s.Key;
            var mi = s.Value;
            return string.Format(
                "\t\t-{0}:{2}{3}:{4}",
                a.Names.Format("|"),
                a.Mandatory ? "required" : "optional",
                ToHelpString(mi.GetUnderlyingType()),
                a.Translator == CommandLineSwitchAttributeTranslators.None ? "" : string.Format("({0})", a.Translator),
                a.Description
                );
        }

        public static string GetUsage(Type t, string newLine = "\n", Func<KeyValuePair<CommandLineSwitchAttribute, MemberInfo>, int, string> formatter = null)
        {
            Requires.NonNull(t, nameof(t));
            var ti = t.GetTypeInfo();
            var switches = CommandLineSwitchAttribute.Find(t).OrderBy(z => z.Key.Mandatory ? 0 : 1).ThenBy(z => z.Key.Names.OrderBy().First()).ThenBy(z => z.Key.Names[0]);
            var mandates = ti.GetCustomAttributes(typeof(CommandLineMandateAttribute), true).OfType<CommandLineMandateAttribute>();
            if (formatter == null)
            {
                if (mandates.HasData())
                {
                    formatter = MandatesUsageSwitchFormatter;
                }
                else
                {
                    formatter = SimpleUsageSwitchFormatter;
                }
            }
            if (mandates.HasData())
            {
                string indent = "\t";
                var modeSwitch = switches.First(s => s.Key is CommandLineSwitchModeSwitchAttribute);
                var sb = new StringBuilder();
                foreach (var m in mandates)
                {
                    foreach (int enumVal in m.EnumVals)
                    {
                        var mandatoryKeys = m.MandatoryKeys.ToCaseInsensitiveSet();
                        var optionalKeys = m.OptionalKeys.ToCaseInsensitiveSet();
                        optionalKeys.Remove(mandatoryKeys);
                        var enumName = Enum.Parse(modeSwitch.Value.GetUnderlyingType(), enumVal.ToString()).ToString();
                        sb.AppendFormat("{3}{0}{1}Mandatory Args:{0}{1}{1}-{2}:{3}{0}", newLine, indent, modeSwitch.Key.Names[0], enumName);
                        var mySwitches = switches.Where(s => mandatoryKeys.ContainsAnyElement(s.Key.Names));
                        if (mySwitches.HasData())
                        {
                            sb.Append(mySwitches.Format(newLine, formatter));
                        }
                        mySwitches = switches.Where(s => optionalKeys.ContainsAnyElement(s.Key.Names));
                        if (mySwitches.HasData())
                        {
                            sb.AppendFormat("{1}Optional Args:{0}", newLine, indent);
                            sb.Append(mySwitches.Format(newLine, formatter));
                        }
                        sb.Append(newLine);
                    }
                }
                return sb.ToString();
            }
            else
            {
                return switches.Format(newLine, formatter);
            }
        }

        public static void SetArgs(CommandLineInfo cli, object o, bool throwOnDuplicateArgs = true, bool throwOnExtraNonMappedArgs = false)
        {
            Requires.NonNull(cli, nameof(cli));
            Requires.NonNull(o, nameof(o));

            var argsSeen = new HashSet<CommandLineInfo.Arg>();
            var switchesSeen = new HashSet<CommandLineSwitchAttribute>();
            var zs = CommandLineSwitchAttribute.Find(o.GetType());
            var attributesFound = new HashSet<CommandLineSwitchAttribute>();
            string mode = null;
            int modeVal = 0;
            var mandatoryButMissing = new List<CommandLineSwitchAttribute>();
            foreach (var z in zs)
            {
                var a = z.Key;
                var v = new VarInfo(o, z.Value);
                if (switchesSeen.Contains(a))
                {
                    if (throwOnDuplicateArgs)
                    {
                        throw new CommmandLineInfoException(CommandLineInfoExceptionCodes.DuplicateArgsFound);
                    }
                    continue;
                }
                foreach (var name in a.Names)
                {
                    string appSettingsVal = string.IsNullOrEmpty(a.AppSettingsName) ? null : cli.Configuration[a.AppSettingsName];
                    if (cli.ContainsSwitch(name) || !string.IsNullOrEmpty(appSettingsVal))
                    {
                        string s;
                        var arg = cli.ArgsByKey[name].FirstOrDefault();
                        if (arg == null)
                        {
                            s = appSettingsVal;
                        }
                        else
                        {
                            argsSeen.Add(arg);
                            s = arg.Val;
                        }
                        object val = s;
                        if (v.VarType() == typeof(bool))
                        {
                            val = string.IsNullOrEmpty(s) ? true : Parse.ParseBool(s);
                        }
                        if (a.Translator.HasFlag(CommandLineSwitchAttributeTranslators.Csv))
                        {
                            val = CSV.ParseLine(s);
                        }
                        else if (a.Translator.HasFlag(CommandLineSwitchAttributeTranslators.Csints))
                        {
                            val = CSV.ParseIntegerRow(s);
                        }
                        else if (a.Translator.HasFlag(CommandLineSwitchAttributeTranslators.NameValuePairs))
                        {
                            var parts = CSV.ParseLine(s);
                            var d = new Dictionary<string, string>();
                            foreach (var part in parts)
                            {
                                string l, r;
                                StringHelpers.Split(part, "=", true, out l, out r);
                                d[l.Trim()] = r.Trim();
                            }
                            val = d;
                        }
                        else if (a.Translator.HasFlag(CommandLineSwitchAttributeTranslators.FilePath))
                        {
                            val = s.Contains("%") ? Environment.ExpandEnvironmentVariables(s) : s;
                            val = Path.GetFullPath(val as string);
                        }
                        else if (a.Translator.HasFlag(CommandLineSwitchAttributeTranslators.Url))
                        {
                            val = (new Uri(s)).ToString();
                        }
                        else if (a.Translator.HasFlag(CommandLineSwitchAttributeTranslators.FilePathOrUrl))
                        {
                            if (Uri.TryCreate(s, UriKind.Absolute, out Uri u)&&u.Scheme!=WebHelpers.CommonSchemes.File)
                            {
                                val = (new Uri(s)).ToString();
                            }
                            else
                            {
                                val = s.Contains("%") ? Environment.ExpandEnvironmentVariables(s) : s;
                                val = Path.GetFullPath(val as string);
                            }
                        }
                        v.Val = val;
                        if (a is CommandLineSwitchModeSwitchAttribute)
                        {
                            modeVal = (int)v.Val;
                            mode = v.ValAsString();
                        }
                        attributesFound.Add(a);
                        goto Found;
                    }
                }
                if (a.Mandatory)
                {
                    mandatoryButMissing.Add(a);
                }
                Found:
                Stuff.Noop();
            }
            foreach (var a in mandatoryButMissing)
            {
                if (a.IsMandatoryForMode(mode))
                {
                    throw new CommmandLineInfoException(CommandLineInfoExceptionCodes.MissingMandatoryArg, a.ToString());
                }
            }
            if (throwOnExtraNonMappedArgs)
            {
                var extra = cli.ArgsByPos.FirstOrDefault(a => a.IsSwitch && !argsSeen.Contains(a));
                if (extra != null)
                {
                    throw new CommmandLineInfoException(CommandLineInfoExceptionCodes.ExtraNonMappedArgsFound, extra.ToString());
                }
            }

            var mandates = o.GetType().GetTypeInfo().GetCustomAttributes(typeof(CommandLineMandateAttribute), true).OfType<CommandLineMandateAttribute>();
            if (mandates.HasData())
            {
                var foundKeys = attributesFound.ConvertAll(a => a.Key.Trim()).ToCaseInsensitiveSet();
                foreach (var mandate in mandates)
                {
                    if (!mandate.EnumVals.Contains(modeVal)) continue;
                    foreach (var key in mandate.MandatoryKeys)
                    {
                        var parts = key.Split('|');
                        foreach (var part in parts)
                        {
                            if (foundKeys.Contains(part.Trim())) goto NextKey;
                        }
                        goto NextMandate;
                        NextKey:
                        Stuff.Noop();
                    }
                    return;
                    NextMandate:
                    Stuff.Noop();
                }
                throw new CommmandLineInfoException(CommandLineInfoExceptionCodes.NoMandatesMet);
            }
        }
    }
}
