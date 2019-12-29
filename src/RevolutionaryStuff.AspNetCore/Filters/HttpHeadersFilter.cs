using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using RevolutionaryStuff.Core;

namespace RevolutionaryStuff.AspNetCore.Filters
{
    public class HttpHeadersFilter : IAsyncActionFilter
    {
        public class Config
        {
            public const string ConfigSectionName = "HttpHeadersFilterConfig";

            public bool IncludeMachineName { get; set; }
            public bool IncludeServerTime { get; set; }
            public bool IncludeEnvironmentInformation { get; set; }
            public TimeSpan IdleLogout { get; set; }
            public IDictionary<string, string> AdditionalHeaders { get; set; }
        }

        private readonly IOptions<Config> ConfigOptions;
        private readonly IWebHostEnvironment Host;

        public HttpHeadersFilter(IOptions<Config> configOptions, IWebHostEnvironment host)
        {
            Requires.NonNull(configOptions, nameof(configOptions));
            ConfigOptions = configOptions;
            Host = host;
        }

        async Task IAsyncActionFilter.OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var c = ConfigOptions.Value;
            var headers = context.HttpContext.Response.Headers;
            if (!headers.ContainsKey(WebHelpers.HeaderStrings.CacheControl))
            {
                headers[WebHelpers.HeaderStrings.CacheControl] = "no-cache, no-store";
            }
            if (c.IncludeMachineName)
            {
                headers["x-MachineName"] = Environment.MachineName;
            }
            if (c.IncludeServerTime)
            {
                headers["x-ServerTime"] = DateTime.Now.ToString();
                headers["x-ServerTimeUtc"] = DateTime.UtcNow.ToString();
            }
            if (c.IncludeEnvironmentInformation)
            {
                headers["x-Environment"] = Host.EnvironmentName;
                headers["x-ApplicationName"] = Host.ApplicationName;
            }
            if (c.AdditionalHeaders != null)
            {
                foreach (var kvp in c.AdditionalHeaders)
                {
                    headers[kvp.Key] = kvp.Value;
                }
            }
            context.HttpContext.Response.Cookies.Append("loggedIn", context.HttpContext.User.Identity.IsAuthenticated ? "true" : "false");
            context.HttpContext.Response.Cookies.Append("sessionTimeoutInSeconds", c.IdleLogout.TotalSeconds.ToString());
            await next();
        }
    }
}
