using Infrastructure.Configuration.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Configuration.StartupFilters;

public class VersionStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.Map("/version", builder => builder.UseMiddleware<VesrionMiddleware>());
            next(app);
        };
    }
}