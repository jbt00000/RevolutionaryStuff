using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevolutionaryStuff.ApiCore.Startup;

namespace RevolutionaryStuff.ApiCore.Tests.Startup;

[TestClass]
public class ApiProgramCorsTests
{
    [TestMethod]
    public async Task Cors_WrapsAuthorizationAndPreservesProtectedEndpointsAsync()
    {
        var program = new CorsTestProgram();
        var run = Task.Run(() => program.GoAsync([]));
        var app = await program.Started.Task.WaitAsync(TimeSpan.FromSeconds(30));
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
            using var preflight = new HttpRequestMessage(HttpMethod.Options, "/protected");
            preflight.Headers.Add("Origin", "https://localhost:7168");
            preflight.Headers.Add("Access-Control-Request-Method", "GET");
            preflight.Headers.Add("Access-Control-Request-Headers", "authorization,x-traffk-tenantid");
            using var preflightResponse = await client.SendAsync(preflight);
            Assert.AreEqual(HttpStatusCode.NoContent, preflightResponse.StatusCode);
            AssertCorsHeaders(preflightResponse);
            StringAssert.Contains(string.Join(",", preflightResponse.Headers.GetValues("Access-Control-Allow-Headers")), "authorization");

            foreach (var (identity, expectedStatus) in new[]
            {
                ("", HttpStatusCode.Unauthorized),
                ("reader", HttpStatusCode.Forbidden),
                ("dashboard-user", HttpStatusCode.OK)
            })
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "/protected");
                request.Headers.Add("Origin", "https://localhost:7168");
                if (identity.Length > 0)
                    request.Headers.Add("Test-Identity", identity);
                using var response = await client.SendAsync(request);
                Assert.AreEqual(expectedStatus, response.StatusCode, identity);
                AssertCorsHeaders(response);
            }
        }
        finally
        {
            await app.StopAsync();
            await run;
            await app.DisposeAsync();
        }
    }

    private static void AssertCorsHeaders(HttpResponseMessage response)
    {
        Assert.IsTrue(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins),
            $"HTTP {(int)response.StatusCode} must be readable by the browser.");
        Assert.AreEqual("*", origins.Single());
    }

    private sealed class CorsTestProgram : ApiProgram
    {
        public TaskCompletionSource<WebApplication> Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override void SetupConfiguration(WebApplicationBuilder builder, string[] args) { }
        protected override void ConfigureBuilder(WebApplicationBuilder builder)
            => builder.WebHost.UseUrls("http://127.0.0.1:0");

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddProblemDetails();
            services.AddAuthentication("test").AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("test", _ => { });
            services.AddAuthorization(options => options.AddPolicy("dashboard", policy => policy.RequireClaim("dashboard", "read")));
            services.AddCors(options => options.AddPolicy("portal", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
        }

        protected override void UseHttpsRedirection(WebApplication app) { }

        protected override void SetupMiddleware(WebApplication app)
        {
            base.SetupMiddleware(app);
            app.UseCors("portal");
        }

        protected override void MapWebEndpoints(WebApplication app)
        {
            app.MapGet("/protected", () => Results.Ok("dashboard")).RequireAuthorization("dashboard");
            app.Lifetime.ApplicationStarted.Register(() => Started.TrySetResult(app));
        }
    }

    private sealed class TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = Request.Headers["Test-Identity"].ToString();
            if (identity.Length == 0)
                return Task.FromResult(AuthenticateResult.NoResult());

            var claims = identity == "dashboard-user" ? new[] { new Claim("dashboard", "read") } : [];
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
        }
    }
}
