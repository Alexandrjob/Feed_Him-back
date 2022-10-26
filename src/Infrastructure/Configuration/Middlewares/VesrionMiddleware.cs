using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Configuration.Middlewares;

public class VesrionMiddleware
{
    public VesrionMiddleware()
    {
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "no version";
        await context.Response.WriteAsync(version);
    }
}