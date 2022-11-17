using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Infrastructure.Factories;
using TaskTrackerCat.Infrastructure.Factories.Interfaces;
using TaskTrackerCat.Infrastructure.Handlers.Commands;
using TaskTrackerCat.Infrastructure.Handlers.Implementation;
using TaskTrackerCat.Infrastructure.Handlers.Interfaces;
using TaskTrackerCat.Infrastructure.HostedServices;
using TaskTrackerCat.Infrastructure.Identity;
using TaskTrackerCat.Infrastructure.InitServices;
using TaskTrackerCat.Repositories.Implementation;
using TaskTrackerCat.Repositories.Interfaces;

namespace TaskTrackerCat;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        #region DataBase

        services.AddScoped<IDbConnectionFactory<SqlConnection>, MsConnectionFactory>();

        services.AddScoped<IDietRepository, DietRepository>();
        services.AddScoped<IConfigRepository, ConfigRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddTransient<InitService>();
        services.AddTransient<InitDiets, InitDiets>();
        services.AddHostedService<TimedHostedService>();

        #endregion

        services.AddScoped<IRequestHandler<UpdateConfigCommand>, UpdateConfigHandler>();
        services
            .AddScoped<IRequestAuthenticationHandler<AuthenticationUserViewModel, AuthenticationUserViewModel>,
                AuthenticationUserHandler>();
        services
            .AddScoped<IRequestAuthorizeHandler<AuthorizeUserViewModel, AuthorizeUserViewModel>,
                AuthorizeUserHandler>();
        services.AddTransient<JwtTokenHelper, JwtTokenHelper>();


        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins("http://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });
        });

        services.AddAuthorization();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = AuthOptions.ISSUER,
                    ValidateAudience = true,
                    ValidAudience = AuthOptions.AUDIENCE,
                    ValidateLifetime = true,
                    IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true
                };
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseCors(builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}