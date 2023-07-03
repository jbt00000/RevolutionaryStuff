using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace RevolutionaryStuff.Data.Cosmos;

#pragma warning disable IDE1006 // Naming Styles
public static class _Use
#pragma warning restore IDE1006 // Naming Styles
{
    public class Settings
    {
    }

    private static int InitCalls;
    public static void UseRevolutionaryStuffDataCosmos(this IServiceCollection services, Settings? settings = null)
    {
        if (Interlocked.Increment(ref InitCalls) > 1) return;
    }
}
