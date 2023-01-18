using MediatR;
using Microsoft.Data.SqlClient;
using TaskTrackerCat.API.Filters;
using TaskTrackerCat.BLL.Mapping;
using TaskTrackerCat.BLL.Services;
using TaskTrackerCat.BLL.Services.Helpers;
using TaskTrackerCat.BLL.SignalR;
using TaskTrackerCat.DAL.Factories;
using TaskTrackerCat.DAL.Factories.Interfaces;
using TaskTrackerCat.DAL.Repositories.Implementation;
using TaskTrackerCat.DAL.Repositories.Interfaces;

namespace TaskTrackerCat.API;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        #region DataBase

        services.AddScoped<IDbConnectionFactory<SqlConnection>, MsConnectionFactory>();
        services.AddScoped<SqlConnectionFilter>();

        services.AddScoped<IDietRepository, DietRepository>();
        services.AddScoped<IConfigRepository, ConfigRepository>();

        services.AddTransient<ConfigHelper, ConfigHelper>();
        services.AddScoped<UpdateConfigService, UpdateConfigService>();

        services.AddTransient<InitService>();
        services.AddHostedService<TimedHostedService>();

        #endregion

        services.AddMemoryCache();
        services.AddSingleton<DietHub, DietHub>();

        services.AddSignalR();
        services.AddAutoMapper(typeof(MappingProfile));
        services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        if (env.IsDevelopment())
            app.UseCors(builder =>
            {
                builder.WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<DietHub>("/hub");
            endpoints.MapDefaultControllerRoute();
        });
    }
}