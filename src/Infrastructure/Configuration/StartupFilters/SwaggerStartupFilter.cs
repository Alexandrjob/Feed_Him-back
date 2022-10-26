using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Configuration.StartupFilters;

public class SwaggerStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            next(app);
        };
    }
}