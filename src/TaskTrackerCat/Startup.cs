using Microsoft.Data.SqlClient;
using Serilog;
using Serilog.Formatting.Compact;
using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Infrastructure;
using TaskTrackerCat.Infrastructure.Factories;
using TaskTrackerCat.Infrastructure.Factories.Interfaces;
using TaskTrackerCat.Infrastructure.Handlers;
using TaskTrackerCat.Infrastructure.Handlers.Interfaces;
using TaskTrackerCat.Repositories.Implementation;
using TaskTrackerCat.Repositories.Interfaces;

namespace TaskTrackerCat;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        #region DataBase

        //services.AddScoped<IDbConnectionFactory<NpgsqlConnection>, NpgsqlConnectionFactory>();
        services.AddScoped<IDbConnectionFactory<SqlConnection>, MsConnectionFactory>();

        services.AddScoped<IDietRepository, DietRepository>();
        services.AddScoped<IConfigRepository, ConfigRepository>();

        services.AddTransient<InitService>();
        services.AddHostedService<TimedHostedService>();

        #endregion

        services.AddMemoryCache();
        services.AddSignalR();

        services.AddSingleton<DietHub, DietHub>();
        services.AddScoped<IRequestHandler<ConfigViewModel>, UpdateConfigHandler>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        if (env.IsDevelopment())
        {
            // loggerFactory.AddSerilog(new LoggerConfiguration()
            //     .MinimumLevel.Information()
            //     .WriteTo.Console()
            //     .WriteTo.Seq("http://localhost:5341")
            //     .CreateLogger());

            app.UseCors(builder =>
            {
                builder.WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        }

        loggerFactory.AddSerilog(new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(new CompactJsonFormatter(), "Logs/logs.json")
            .CreateLogger());

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<DietHub>("/hub");
            endpoints.MapDefaultControllerRoute();
        });
    }
}