using Infrastructure.Configuration.Extensions;
using Infrastructure.Configuration.StartupFilters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public static class HostBuilderExtensions
{
    public static IHostBuilder AddInfrastructure(this IHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddSerilogFile();
        });

        builder.ConfigureServices(services =>
        {
            services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());

            services.AddSingleton<IStartupFilter, RequestLoggingStartupFilter>();
            services.AddSingleton<IStartupFilter, SwaggerStartupFilter>();
            services.AddSwaggerGen();
        });

        return builder;
    }
}