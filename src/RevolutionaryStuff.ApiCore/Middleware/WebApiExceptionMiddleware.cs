using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RevolutionaryStuff.ApiCore.Middleware;

public class WebApiExceptionMiddleware(IOptions<WebApiExceptionMiddleware.Config> _configOptions, RequestDelegate _next, ILogger<WebApiExceptionMiddleware> _logger)
{
    public class Config
    {
        public const string ConfigSectionName = "WebApiExceptionMiddleware";

        public bool SendExceptionError { get; set; }
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var config = _configOptions.Value;
        var response = context.Response;
        response.ContentType = MimeType.Application.Json.PrimaryContentType;

        // Determine response status code based on exception type
        var statusCode = exception switch
        {
            ArgumentException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            KeyNotFoundException => HttpStatusCode.NotFound,
            _ => HttpStatusCode.InternalServerError
        };

        var errorResponse = new
        {
            message = "An error occurred while processing your request.",
            errorCode = (int)statusCode,
            details = statusCode == HttpStatusCode.InternalServerError ? null : exception.Message, // Avoid exposing internal errors
            ee = config.SendExceptionError ? new ExceptionError(exception) : null
        };

        response.StatusCode = (int)statusCode;
        await response.WriteAsync(JsonHelpers.ToMicrosoftJson(errorResponse));
    }
}
