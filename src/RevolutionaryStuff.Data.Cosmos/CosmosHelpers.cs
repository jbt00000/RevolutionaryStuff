using System.Threading;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos.Scripts;

namespace RevolutionaryStuff.Data.Cosmos;

public static class CosmosHelpers
{
    public static async Task<T?> GetFirstOrDefaultAsync<T>(this IQueryable<T> q, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(q);

        try
        {
            using var fi = q.Take(1).ToFeedIterator();
            ArgumentNullException.ThrowIfNull(fi);
            while (fi.HasMoreResults)
            {
                var resp = await fi.ReadNextAsync(cancellationToken);
                //at least we can set a breakpoint here to look at items inside of the FeedResponse like cost!
                return resp.FirstOrDefault();
            }
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // This is expected and suppressed
        }
        return default;
    }

    public static async Task<int> ExecuteForEachAsync<T>(this IQueryable<T> q, Func<T, Task> executeAsync, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(q);
        ArgumentNullException.ThrowIfNull(executeAsync);

        var count = 0;
        try
        {
            using var fi = q.ToFeedIterator();
            ArgumentNullException.ThrowIfNull(fi);
            while (fi.HasMoreResults)
            {
                var resp = await fi.ReadNextAsync(cancellationToken);
                foreach (var item in resp)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    await executeAsync(item);
                    ++count;
                }
            }
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // This is expected and suppressed
        }
        return count;
    }

    public static async Task<IReadOnlyList<T>> GetAllItemsAsync<T>(this IQueryable<T> q, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(q);

        List<T> items = new();

        try
        {
            using var fi = q.ToFeedIterator();
            ArgumentNullException.ThrowIfNull(fi);
            while (fi.HasMoreResults)
            {
                var resp = await fi.ReadNextAsync(cancellationToken);
                //at least we can set a breakpoint here to look at items inside of the FeedResponse like cost!
                items.AddRange(resp);
            }
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Stuff.Noop();
            // This is expected and suppressed
        }
        return items.AsReadOnly();
    }

    public static async Task<StoredProcedureResponse> UpsertStoredProcedureAsync(this Scripts scripts, StoredProcedureProperties properties)
    {
        try
        {
            return await scripts.ReplaceStoredProcedureAsync(properties);
        }
        catch (Exception)
        {
            return await scripts.CreateStoredProcedureAsync(properties);
        }
    }

    public static async Task<TriggerResponse> UpsertTriggerAsync(this Scripts scripts, TriggerProperties properties)
    {
        try
        {
            return await scripts.ReplaceTriggerAsync(properties);
        }
        catch (Exception)
        {
            return await scripts.CreateTriggerAsync(properties);
        }
    }

    private class MyContinuationToken
    {
        public int TotalCount { get; set; }
        public string? CosmosContinuationToken { get; set; }
        public static MyContinuationToken? Decode(string s)
        {
            if (s != null)
            {
                try
                {
                    s = s.DecodeBase64String();
                    var parts = CSV.ParseLine(s);
                    if (parts.Length == 2)
                    {
                        return new()
                        {
                            TotalCount = int.Parse(parts[0]),
                            CosmosContinuationToken = parts[1]
                        };
                    }
                }
                catch (Exception) { }
            }
            return null;
        }

        public string GetToken()
            => CSV.FormatLine(new[] { TotalCount.ToString(), CosmosContinuationToken }, false).ToBase64String();
    }

    public static Task<ItemResponse<T>> PatchItemAsync<T>(this Container container, string id, string partitionKey, IReadOnlyList<PatchOperation> patchOperations, PatchItemRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
        => container.PatchItemAsync<T>(id, new(partitionKey), patchOperations, requestOptions, cancellationToken);
}
