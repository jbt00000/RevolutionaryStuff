using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RevolutionaryStuff.Core.Diagnostics;

public class TraceRegion : BaseDisposable
{
    private readonly string Name;
    private readonly Stopwatch Stopwatch;

    #region Constructors

    public TraceRegion([CallerMemberName] string name = null, params object[] args)
    {
        Name = name;
        if (!string.IsNullOrEmpty(Name))
        {
            var s = $"{Name} vvvvvvvvvvvvvvvvvvvvvvvv";
            Trace.WriteLine(s);
        }
        Trace.Indent();
        Stopwatch = Stopwatch.StartNew();
    }

    #endregion

    protected override void OnDispose(bool disposing)
    {
        string timing = null;
        if (Stopwatch != null)
        {
            Stopwatch.Stop();
            timing = $" duration={Stopwatch.Elapsed}";
        }
        Trace.Unindent();
        if (!string.IsNullOrEmpty(Name))
        {
            var s = $"{Name} ^^^^^^^^^^^^^^^^^^^^^^^^{timing}";
            Trace.WriteLine(s);
        }
        base.OnDispose(disposing);
    }

    public static void Call(Action a, [CallerMemberName] string name = null, bool catchAndPrintExceptions = true, bool throwExceptions = true)
    {
        using (new TraceRegion(name))
        {
            try
            {
                a();
            }
            catch (Exception ex)
            {
                if (catchAndPrintExceptions)
                {
                    Debug.WriteLine(ex);
                }
                if (throwExceptions)
                {
                    throw;
                }
            }
        }
    }

    public static R Call<R>(Func<R> a, [CallerMemberName] string name = null, bool catchAndPrintExceptions = true)
    {
        using (new TraceRegion(name))
        {
            try
            {
                return a();
            }
            catch (Exception ex)
            {
                if (catchAndPrintExceptions)
                {
                    Trace.WriteLine(ex);
                }
                throw;
            }
        }
    }
}
