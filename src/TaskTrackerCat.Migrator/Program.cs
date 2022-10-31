using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration.GetSection("ConnectionStringMSSQL").Value;

var services = new ServiceCollection()
    .AddFluentMigratorCore()
    .ConfigureRunner(
        rb => rb
            .AddSqlServer()
            .WithGlobalConnectionString(connectionString)
            .ScanIn(typeof(Program).Assembly)
            .For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

var serviceProvider = services.BuildServiceProvider(false);

using (serviceProvider.CreateScope())
{
    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

    runner.MigrateUp();
}