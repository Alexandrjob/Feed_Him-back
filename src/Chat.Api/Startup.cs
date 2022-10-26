using Chat.Api.Infrastructure.Factories;
using Chat.Api.Infrastructure.Factories.Interfaces;
using Chat.Api.Infrastructure.Mapping;
using Chat.Api.Repositories.Implementation;
using Chat.Api.Repositories.Interfaces;
using MediatR;
using Npgsql;

namespace Chat.Api;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        #region DataBase

        services.AddScoped<IDbConnectionFactory<NpgsqlConnection>, NpgsqlConnectionFactory>();
        services.AddScoped<IItemRepository, ItemRepository>();

        #endregion

        services.AddAutoMapper(typeof(MappingProfile));

        services.AddControllers();
        services.AddMediatR(typeof(Startup));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}