using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using RevolutionaryStuff.Core.Collections;

namespace RevolutionaryStuff.Core.ApplicationParts;

public class CommandLineInfo
{
    public const string DefaultSwitchCharacters = "/-\\";
    public readonly string SwitchCharacters;
    public readonly MultipleValueDictionary<string, Arg> ArgsByKey;
    public readonly IList<Arg> ArgsByPos;
    public readonly IList<string> RawArgs;
    public readonly bool SwitchesAreCaseInsensitive;
    public readonly IConfiguration Configuration;

    #region Constructors

    public CommandLineInfo(IConfiguration configuration, string[] args, bool switchesAreCaseInsensitive = true, string switchCharacters = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        Configuration = configuration;
        SwitchesAreCaseInsensitive = switchesAreCaseInsensitive;
        SwitchCharacters = switchCharacters ?? DefaultSwitchCharacters;
        RawArgs = args.AsReadOnly();
        ArgsByKey = Arg.Parse(args, switchesAreCaseInsensitive, SwitchCharacters);
        ArgsByPos = ArgsByKey.AtomEnumerable.ConvertAll(kvp => kvp.Value).OrderBy(z => z.Position).ToList();
    }

    #endregion

    public static string GetUsage(Type t)
    {
        var a = t.GetTypeInfo().Assembly;
        var ai = a.GetInfo();
        var full = a.Location;
        var fn = Path.GetFileName(full);
        var folder = Path.GetDirectoryName(full);
        if (0 == string.Compare(folder, Directory.GetCurrentDirectory(), true))
        {
            full = fn;
        }
        var usage = string.Format(@"
What this does
==============
{0}

Program Path
============
{1}

Arguments:
==========
{2}
",
            ai.Description,
            full,
            CommandLineSwitchAttribute.GetUsage(t)
            );
        return usage;
    }

    public bool ContainsSwitch(string name)
    {
        return ArgsByPos.FirstOrDefault(a => a.IsSwitch && 0 == string.Compare(a.Switch, name, SwitchesAreCaseInsensitive)) != null;
    }

    public string GetVal(int pos)
    {
        return GetVal(pos, null);
    }

    public string GetVal(int pos, string missing)
    {
        if (pos >= ArgsByPos.Count)
        {
            return missing;
        }

        var cla = ArgsByPos[pos];
        return StringHelpers.Coalesce(cla.Val, missing);
    }

    public string GetVal(string key)
    {
        return GetVal(key, null);
    }

    public string GetVal(string key, string missing)
    {
        var cla = ArgsByKey[key].FirstOrDefault();
        return cla == null ? missing : StringHelpers.Coalesce(cla.Val, missing);
    }

    public class Arg
    {
        public const string NoSwitch = "";

        public readonly int Position;
        public readonly string Literal;
        public readonly string Switch;
        public readonly bool IsSwitch;
        public readonly string Val;
        public Arg Next { get; private set; }

        public Arg(int position, string literal, string @switch, string val)
        {
            Position = position;
            Literal = literal;
            Switch = (@switch ?? NoSwitch).Trim();
            IsSwitch = IsSwitch = !string.IsNullOrEmpty(Switch);
            Val = val;
        }

        public static MultipleValueDictionary<string, Arg> Parse(string[] args, bool switchesAreCaseInsensitive, string switchCharacters)
        {
            var orderedArgs = new List<Arg>();
            var m = switchesAreCaseInsensitive
                ? new MultipleValueDictionary<string, Arg>(Comparers.CaseInsensitiveStringComparer)
                : [];
            var pos = 0;
            foreach (var arg in args)
            {
                if (string.IsNullOrEmpty(arg)) continue;
                string s, v;
                if (switchCharacters.Contains(arg[0]))
                {
                    s = arg[1..];
                    if (s.Length == 0) goto NextArg;
                    s.Split(":", true, out s, out v);
                    if (switchesAreCaseInsensitive) s = s.ToLower();
                }
                else
                {
                    s = NoSwitch;
                    v = arg;
                }
                var cla = new Arg(pos++, arg, s, v);
                m.Add(cla.Switch, cla);
                orderedArgs.Add(cla);
NextArg:
                Stuff.Noop();
            }
            for (var z = 0; z < orderedArgs.Count - 1; ++z)
            {
                orderedArgs[z].Next = orderedArgs[z + 1];
            }
            return m;
        }
    }
}
