using Infrastructure.Configuration.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Configuration.StartupFilters;

public class RequestLoggingStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseMiddleware<RequestLoggingMiddleware>();
            next(app);
        };
    }
}