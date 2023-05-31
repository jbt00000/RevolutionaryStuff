using System.Runtime.CompilerServices;

namespace RevolutionaryStuff.Core.Caching;

public static class PermaCache
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, object> ResultByKey = new();

    private static TRes FindOrCreatePrivate<TRes>(string key, Func<TRes> creator)
        => (TRes)ResultByKey.FindOrCreate(key, () => creator());

    internal static TRes FindOrCreate<TRes>(object key0, Func<TRes> creator, [CallerMemberName] string caller = null, [CallerLineNumber] int callerLine = 0)
        => FindOrCreatePrivate(Cache.CreateKey<TRes>(caller, callerLine, key0), creator);

    internal static TRes FindOrCreate<TRes>(object key0, object key1, Func<TRes> creator, [CallerMemberName] string caller = null, [CallerLineNumber] int callerLine = 0)
        => FindOrCreatePrivate(Cache.CreateKey<TRes>(caller, callerLine, key0, key1), creator);

    internal static TRes FindOrCreate<TRes>(object key0, object key1, object key2, Func<TRes> creator, [CallerMemberName] string caller = null, [CallerLineNumber] int callerLine = 0)
        => FindOrCreatePrivate(Cache.CreateKey<TRes>(caller, callerLine, key0, key1, key2), creator);
}
