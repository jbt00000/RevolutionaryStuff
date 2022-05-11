using System.Net.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RevolutionaryStuff.Core.ApplicationParts;
using RevolutionaryStuff.Core.Caching;

namespace RevolutionaryStuff.Core;

public class AzureActiveDirectoryResourceAuthorizationGetter : IBearerGetter
{
    public enum GrantTypes
    {
        Password = 1,
        ClientCredentials = 2,
    }

    private static readonly Uri MicrosoftOAuthLoginEndpointUrl = new("https://login.windows.net/common/oauth2/token");
    private static readonly Uri DefaultLoginUrl = MicrosoftOAuthLoginEndpointUrl;

    public class Config : IValidate
    {
        public const string ConfigSectionName = "AuthorizationConfig";

        public class AuthenticatorConfig : IValidate
        {
            public bool ForceFetchNew { get; set; }

            public string Name { get; set; }

            public Uri LoginUrl { get; set; }

            public string ClientId { get; set; }

            public string ClientSecret { get; set; }

            public string Resource { get; set; }

            public string User { get; set; }

            public string Pass { get; set; }

            public string TenantId { get; set; }

            public GrantTypes GrantType { get; set; }

            internal string CacheKey
                => Cache.CreateKey(LoginUrl, ClientId, ClientSecret, Resource, User, Pass, GrantType);

            public void Validate()
            {
                Requires.False(GrantType == 0, nameof(GrantType));
            }
        }

        public IList<AuthenticatorConfig> Authenticators { get; set; }

        public void Validate()
        {
            Requires.NonNull(Authenticators);
            foreach (var a in Authenticators)
            {
                Requires.Valid(a, nameof(AuthenticatorConfig));
            }
        }
    }

    public class AuthenticationResult
    {
        public static AuthenticationResult CreateFromJson(string json)
            => JsonConvert.DeserializeObject<AuthenticationResult>(json);

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

        [JsonIgnore]
        public bool IsError => Error != null;

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("resource")]
        public string Resource { get; set; }

        [JsonProperty("access_token")]
        public string Token { get; set; }
        /*
                        {
                        "token_type":"Bearer",
                        "scope":"Dashboard.Read.All Dataset.Read.All Group.Read Report.Read.All",
                        "expires_in":"3599",
                        "ext_expires_in":"3600",
                        "expires_on":"1470031631",
                        "not_before":"1470027731",
                        "resource":"https://analysis.windows.net/powerbi/api",
                        "access_token":"eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ik1uQ19WWmNBVGZNNXBPWWlKSE1iYTlnb0VLWSIsImtpZCI6Ik1uQ19WWmNBVGZNNXBPWWlKSE1iYTlnb0VLWSJ9.eyJhdWQiOiJodHRwczovL2FuYWx5c2lzLndpbmRvd3MubmV0L3Bvd2VyYmkvYXBpIiwiaXNzIjoiaHR0cHM6Ly9zdHMud2luZG93cy5uZXQvZGQ2ODEyNTItNDQyZS00MDcwLWJlMzgtMjEyMWY4OTE5YWE3LyIsImlhdCI6MTQ3MDAyNzczMSwibmJmIjoxNDcwMDI3NzMxLCJleHAiOjE0NzAwMzE2MzEsImFjciI6IjEiLCJhbXIiOlsicHdkIl0sImFwcGlkIjoiMWM5NzdkMTMtNzI2My00ZTAwLWIxM2ItYmFmNTIxMDUzMDQ1IiwiYXBwaWRhY3IiOiIxIiwiZmFtaWx5X25hbWUiOiJUaG9tYXMiLCJnaXZlbl9uYW1lIjoiSmFzb24iLCJpcGFkZHIiOiIxMDguMzEuMTAxLjE2MCIsIm5hbWUiOiJKYXNvbiAoVHJhZmZrcG9ydGFsKSIsIm9pZCI6ImU1OGRiNTZlLWFhYjMtNGM3NS05YmU3LTMxNGZkNmJlY2Q2NiIsInB1aWQiOiIxMDAzQkZGRDk5NzVEQUM0Iiwic2NwIjoiRGFzaGJvYXJkLlJlYWQuQWxsIERhdGFzZXQuUmVhZC5BbGwgR3JvdXAuUmVhZCBSZXBvcnQuUmVhZC5BbGwiLCJzdWIiOiJLaEs0cl9lSlp5MnpNbWxiZjlsZGZUTUdqQWExdHZiaUtPek1Tdy04Y3VvIiwidGlkIjoiZGQ2ODEyNTItNDQyZS00MDcwLWJlMzgtMjEyMWY4OTE5YWE3IiwidW5pcXVlX25hbWUiOiJqYXNvbkB0cmFmZmtwb3J0YWwub25taWNyb3NvZnQuY29tIiwidXBuIjoiamFzb25AdHJhZmZrcG9ydGFsLm9ubWljcm9zb2Z0LmNvbSIsInZlciI6IjEuMCJ9.J5_GekxH8sqh3OiFM328TEvKbIhjxmJfdcK9EfYjgpGS5CN_fUShvY5sbE4AHsyp54qqKpm8bO0Es943__ZR11CnM17eduVm_382Pg0h6fS88Z13j_dHbmn77a-i_rwhvwzeV3xnzkucdbnCZbW12HwFxRAFZQYEuUO_V8QvCAYO333tdj21nM4I3mIUS5tWwsX7uLAkNaR4_wde9gA_ngAseWbk6UMlLlMirablJq6SYXGjUXrhLn29MSCDu3O96Vdr3rmZl0221L5fY0zSgtkpJAVPb5P1Slko-kf2P0bBlo6b1h0iVeuCfKAWi5IfOx6qHh9zqjKMVvD9aKuonQ","refresh_token":"AAABAAAA0TWEUN3YUUq5vuCvmnaQiTm5SR80CUy1g3H7M8A3e_KuI7TYnGcfbU83-M9JzmcTvPJMZnANS71g2sHYKhLqwMwL8udw7Hss8QJvWzFEBXyONz4fU30oiHWH4SFHdaMogbyhh2pRgUqU8e5TL3XQ4g3g_7d085_28m1KOYAQh1BByPtwNePpAZw4SAJ6aa0U7yIfVmoC4jxXfxvVtu871DVY5RSzeWkKMs4GXqv86zRn7N0PPHqhvlgjMeMZhcc6jNiX8eU0ATEUNEM0ZVEw2hWfwC6rbDhEH6Mvg6RaOjbODTcq9kwSJQqNMN75eaa90zlnQ3nKi4WP7edxHL5kUVSkdhuOsxWIUDcJf1Gc7W5Qs72Q7kjupqEdCXGMY5GIaeKhbtdO06T3eqbHpGmMtaxwPmzTZsGdDgUKGjsO9udpG2__3pssXwIAI0WpsaPMC3z95Ll0VyhzRlzhebkZ2-YjCoj3tR5SuOOFZSKhgNMXJPzuVVzhnhp4k22FC_WK45AL57PI0rw6TuHZh_ceAcNv3UI5mO2n662aR37yAvfMkXmfrUV4nPUaVDrVVtgiIY-NQTS66dz5xpTy18kKQR1sxs4jEj6o0sIs1vEO_iWKXqVvdsS8g_gkhta5FReNe4dzkLCotlmcca7i9WZFlpLbMvviz5HWW6mN1YoxBP691N242BqT4nufhw0JS9P1itLhmRpkWgfilx3DcsG4hyAA","id_token":"eyJ0eXAiOiJKV1QiLCJhbGciOiJub25lIn0.eyJhdWQiOiIxYzk3N2QxMy03MjYzLTRlMDAtYjEzYi1iYWY1MjEwNTMwNDUiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9kZDY4MTI1Mi00NDJlLTQwNzAtYmUzOC0yMTIxZjg5MTlhYTcvIiwiaWF0IjoxNDcwMDI3NzMxLCJuYmYiOjE0NzAwMjc3MzEsImV4cCI6MTQ3MDAzMTYzMSwiYW1yIjpbInB3ZCJdLCJmYW1pbHlfbmFtZSI6IlRob21hcyIsImdpdmVuX25hbWUiOiJKYXNvbiIsImlwYWRkciI6IjEwOC4zMS4xMDEuMTYwIiwibmFtZSI6Ikphc29uIChUcmFmZmtwb3J0YWwpIiwib2lkIjoiZTU4ZGI1NmUtYWFiMy00Yzc1LTliZTctMzE0ZmQ2YmVjZDY2Iiwic3ViIjoiVlZCV0luR3RvM09Ka1R5LTkxNlp2Nkdlb0pjWGRET2ZncW1wNmhMT1JGYyIsInRpZCI6ImRkNjgxMjUyLTQ0MmUtNDA3MC1iZTM4LTIxMjFmODkxOWFhNyIsInVuaXF1ZV9uYW1lIjoiamFzb25AdHJhZmZrcG9ydGFsLm9ubWljcm9zb2Z0LmNvbSIsInVwbiI6Imphc29uQHRyYWZma3BvcnRhbC5vbm1pY3Jvc29mdC5jb20iLCJ2ZXIiOiIxLjAifQ."
                        }
        */

        [JsonProperty("expires_in")]
        public int ExpiresInSeconds { get; set; }

        private readonly DateTimeOffset CreatedAt = DateTimeOffset.Now;

        [JsonIgnore]
        public DateTimeOffset ExpiresAt
            => CreatedAt.AddSeconds(ExpiresInSeconds);

        [JsonIgnore]
        public bool IsExpired
            => DateTimeOffset.Now > ExpiresAt;

        /*

        [JsonProperty("ext_expires_in")]
        public string ext_expires_in { get; set; }

        [JsonProperty("expires_on")]
        public string expires_on { get; set; }

        [JsonProperty("not_before")]
        public string not_before { get; set; }
        */
    }

    private readonly IOptions<Config> ConfigOptions;
    private readonly ICacher ResultCache;

    public AzureActiveDirectoryResourceAuthorizationGetter(IOptions<Config> configOptions, ICacher resultCache = null)
    {
        Requires.NonNull(configOptions);

        ConfigOptions = configOptions;
        ResultCache = resultCache ?? Cache.Passthrough;
    }

    public AzureActiveDirectoryResourceAuthorizationGetter(Config config, ICacher resultCache = null)
        : this(new OptionsWrapper<Config>(config), resultCache)
    { }

    public AzureActiveDirectoryResourceAuthorizationGetter(Config.AuthenticatorConfig config, ICacher resultCache = null)
        : this(new OptionsWrapper<Config>(new Config
        {
            Authenticators = new[] { config }
        }), resultCache)
    { }

    public AzureActiveDirectoryResourceAuthorizationGetter(string clientId, string clientSecret, ICacher resultCache = null)
        : this(new Config.AuthenticatorConfig
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            GrantType = GrantTypes.ClientCredentials,
        }, resultCache)
    { }

    public AzureActiveDirectoryResourceAuthorizationGetter(string clientId, string clientSecret, string resource, string user, string pass, ICacher resultCache = null)
        : this(new Config.AuthenticatorConfig
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            Resource = resource,
            User = user,
            Pass = pass,
            GrantType = GrantTypes.Password,
        }, resultCache)
    { }

    public async Task<AuthenticationResult> AuthorizeAsync(string name = null, bool forceFetchNew = false)
    {
        var config = ConfigOptions.Value;
        Requires.Valid(config);

        var authenticatorConfig = name == null ? config.Authenticators[0] : config.Authenticators.FirstOrDefault(ac => ac.Name == name);
        Requires.NonNull(authenticatorConfig, $"{nameof(config.Authenticators)}['{name}']");

        var datas = new Dictionary<string, string>();
        switch (authenticatorConfig.GrantType)
        {
            case GrantTypes.Password:
                datas["grant_type"] = "password";
                datas["client_secret"] = authenticatorConfig.ClientSecret;
                datas["client_id"] = authenticatorConfig.ClientId;
                datas["resource"] = authenticatorConfig.Resource;
                datas["username"] = authenticatorConfig.User;
                datas["password"] = authenticatorConfig.Pass;
                datas["scope"] = "openid";
                break;
            case GrantTypes.ClientCredentials:
                datas["grant_type"] = "client_credentials";
                datas["client_info"] = "1";
                datas["resource"] = authenticatorConfig.Resource;
                datas["client_secret"] = authenticatorConfig.ClientSecret;
                datas["client_id"] = authenticatorConfig.ClientId;
                break;
            default:
                throw new UnexpectedSwitchValueException(authenticatorConfig.GrantType);
        }
        foreach (var key in datas.Where(kvp => string.IsNullOrEmpty(kvp.Value)).Select(kvp => kvp.Key).ToList())
        {
            datas.Remove(key);
        }

        var res = await ResultCache.FindOrCreateValueAsync(authenticatorConfig.CacheKey, async () =>
        {
            var client = new HttpClient();
            var response = await client.PostAsync(authenticatorConfig.LoginUrl ?? DefaultLoginUrl, WebHelpers.CreateHttpContent(datas));
            var json = await response.Content.ReadAsStringAsync();
            return AuthenticationResult.CreateFromJson(json);
        }, null, authenticatorConfig.ForceFetchNew || forceFetchNew);

        return res;
    }

    async Task<string> IBearerGetter.GetBearerAsync(string name)
    {
        var auth = await AuthorizeAsync(name);
        if (auth.TokenType == "Bearer")
        {
            return auth.Token;
        }
        return null;
    }
}
