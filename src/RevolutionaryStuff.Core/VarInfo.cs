using System.Reflection;

namespace RevolutionaryStuff.Core;

internal class VarInfo
{
    public readonly object Basevar;
    public readonly bool CanWrite;
    public readonly string FullName;
    public readonly MemberInfo Mi;
    public readonly object ValOrig;

    public Type VarType()
    {
        return Mi.GetUnderlyingType();
    }

    internal VarInfo(string FullName, object basevar, MemberInfo mi)
    {
        this.FullName = FullName;
        Basevar = basevar ?? throw new ArgumentNullException("basevar");
        Mi = mi ?? throw new ArgumentNullException("mi");
        CanWrite = mi.CanWrite();
        if (null == basevar) return;
        try
        {
            ValOrig = Val;
        }
        catch (Exception)
        {
        }
    }

    public VarInfo(object basevar, MemberInfo mi) : this(mi.Name, basevar, mi)
    {
    }

    internal static string ValAsString(object o)
    {
        if (null == o) return null;
        var t = o.GetType();
        if (t.FullName == "System.Byte[]")
        {
            var buf = (byte[])o;
            return Raw.Buf2HexString(buf, 0, buf.Length, true, false);
        }

        return o.ToString();
    }

    public string ValAsString()
    {
        return ValAsString(Val);
    }

    public object Val
    {
        get { return Mi.GetValue(Basevar); }
        set
        {
            if (!CanWrite)
            {
                return;
            }
            Mi.SetValue(Basevar, value);
        }
    }
}
