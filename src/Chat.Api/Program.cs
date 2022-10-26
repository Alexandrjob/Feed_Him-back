using Chat.Api;
using Infrastructure;

var host = CreateHostBuilder(args).Build();
using var scope = host.Services.CreateScope();
await host.RunAsync();

static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
        .AddInfrastructure();
}