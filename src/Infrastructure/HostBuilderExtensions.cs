using Infrastructure.Configuration.StartupFilters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class HostBuilderExtensions
{
    public static IHostBuilder AddInfrastructure(this IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());

            //services.AddSingleton<IStartupFilter, VersionStartupFilter>();
            services.AddSingleton<IStartupFilter, RequestLoggingStartupFilter>();
            services.AddSingleton<IStartupFilter, SwaggerStartupFilter>();
            services.AddSwaggerGen();
        });

        return builder;
    }
}