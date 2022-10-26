using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Configuration.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await LogRequest(context);
        await _next(context);
    }

    private async Task LogRequest(HttpContext context)
    {
        try
        {
            _logger.LogInformation($"Http request {context.Request.Path}");
            if (context.Request.ContentLength is < 0 or null)
            {
                return;
            }

            context.Request.EnableBuffering();

            var buffer = new byte[context.Request.ContentLength.Value];
            await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);

            _logger.LogInformation($"Http Request Information:{Environment.NewLine}" +
                                   $"Schema:{context.Request.Scheme} " +
                                   $"Host: {context.Request.Host} " +
                                   $"Path: {context.Request.Path} " +
                                   $"QueryString: {context.Request.QueryString}\r\n " +
                                   $"Headers: {string.Join("\r\n    ", context.Request.Headers)}\r\n" +
                                   $"Request Body: {bodyAsText}");
            context.Request.Body.Position = 0;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not request body");
        }
    }
}