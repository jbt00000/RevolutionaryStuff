using System.Globalization;
using System.Net.Http;

namespace RevolutionaryStuff.Storage.Providers.Azure.Blob;

internal static class AzureRestHelpers
{
    /// <remarks>
    /// http://sql.pawlikowski.pro/2019/03/10/connecting-to-azure-data-lake-storage-gen2-from-powershell-using-rest-api-a-step-by-step-guide/
    /// https://stackoverflow.com/questions/55300772/how-to-rename-a-file-in-blob-storage-by-using-azure-datalake-gen2-rest-api
    /// </remarks>
    public static async Task RenameAsync(string accountName, string base64credentials, Uri sourceAbsoluteUrl, Uri destinationAbsoluteUrl, IDictionary<string, string> kvps, long? itemSize = null)
    {
        using var m = new HttpRequestMessage(HttpMethod.Put, destinationAbsoluteUrl);
        var now = DateTime.UtcNow;
        m.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
        m.Headers.Add("x-ms-rename-source", sourceAbsoluteUrl.ToString());
        m.Headers.Add("x-ms-version", "2018-11-09");
        m.Headers.Add("x-ms-client-request-id", Guid.NewGuid().ToString());
        m.Headers.Add("x-ms-content-length", itemSize.ToString());
        foreach (var kvp in kvps)
        {
            m.Headers.Add(kvp.Key, kvp.Value);
        }
        m.Headers.Authorization = AzureStorageAuthenticationHelper.GetAuthorizationHeader(accountName, base64credentials, now, m);
        using var client = new HttpClient();
        var resp = await client.SendAsync(m);
        resp.EnsureSuccessStatusCode();
    }
}

public class AzureRestRequestor
{
    public static string UserAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4017.0 Safari/537.36 Edg/81.0.389.2";

    internal readonly string AccountName;
    internal readonly string Base64Credentials;
    private readonly Func<HttpClient> HttpClientCreator;

    public AzureRestRequestor(string accountName, string base64credentials, Func<HttpClient> httpClientCreator = null)
    {
        AccountName = accountName;
        Base64Credentials = base64credentials;
        HttpClientCreator = httpClientCreator ?? (() => new HttpClient());
    }

    /// <remarks>
    /// http://sql.pawlikowski.pro/2019/03/10/connecting-to-azure-data-lake-storage-gen2-from-powershell-using-rest-api-a-step-by-step-guide/
    /// https://stackoverflow.com/questions/55300772/how-to-rename-a-file-in-blob-storage-by-using-azure-datalake-gen2-rest-api
    /// </remarks>
    public Task RenameAsync(Uri sourceAbsoluteUrl, Uri destinationAbsoluteUrl, IDictionary<string, string> kvps, long? itemSize = null)
    {
        var d = new Dictionary<string, string>();
        if (kvps.Count > 0)
        {
            foreach (var kvp in kvps)
            {
                d[kvp.Key] = kvp.Value;
            }
        }
        d["x-ms-rename-source"] = sourceAbsoluteUrl.ToString();
        d["x-ms-content-length"] = itemSize.ToString();
        return PutAsync(destinationAbsoluteUrl, d);
    }

    public async Task PutAsync(Uri resourceUrl, IDictionary<string, string> kvps = null, HttpContent content = null)
    {
        using var m = new HttpRequestMessage(HttpMethod.Put, resourceUrl);
        var now = DateTime.UtcNow;
        m.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
        m.Headers.Add("x-ms-version", "2018-11-09");
        m.Headers.Add("x-ms-client-request-id", Guid.NewGuid().ToString());
        m.Headers.Add("Accept", "*/*");
        if (UserAgentString != null)
            m.Headers.UserAgent.TryParseAdd(UserAgentString);
        if (kvps != null)
        {
            foreach (var kvp in kvps)
            {
                m.Headers.Add(kvp.Key, kvp.Value);
            }
        }
        m.Headers.Authorization = AzureStorageAuthenticationHelper.GetAuthorizationHeader(AccountName, Base64Credentials, now, m);
        m.Content = content;
        using var client = HttpClientCreator();
        var resp = await client.SendAsync(m);
        resp.EnsureSuccessStatusCode();
    }
}
