using Dapper;
using Microsoft.Data.SqlClient;
using TaskTrackerCat.Infrastructure.Factories.Interfaces;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Repositories.Implementation;

public class ConfigRepository : IConfigRepository
{
    private readonly IDbConnectionFactory<SqlConnection> _dbConnectionFactory;

    public ConfigRepository(IDbConnectionFactory<SqlConnection> dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public async Task<ConfigDto> GetConfigAsync(ConfigDto config)
    {
        var sql =
            @"SELECT * FROM config " +
            "WHERE id = @Id";

        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<ConfigDto>(sql, config);

        return result.FirstOrDefault();
    }

    public Task<ConfigDto> AddConfigAsync()
    {
        throw new NotImplementedException();
    }

    public async Task UpdateConfigAsync(ConfigDto config)
    {
        var sql = "UPDATE config " +
                  "SET number_meals_per_day = @NumberMealsPerDay " +
                  "WHERE Id = @Id";

        var connection = await _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, config);
    }
}