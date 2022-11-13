using System.Data;
using Microsoft.Data.SqlClient;
using Npgsql;
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

        services.AddScoped<IRequestHandler<ConfigViewModel>, UpdateConfigHandler>();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins("http://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseCors(builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        app.UseRouting();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}