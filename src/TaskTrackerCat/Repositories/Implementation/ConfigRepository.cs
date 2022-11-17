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
            @"SELECT * FROM configs " +
            "WHERE id = @Id";

        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<ConfigDto>(sql, config);

        return result.FirstOrDefault();
    }

    public async Task<ConfigDto> GetConfigFromGroupAsync(GroupDto group)
    {
        var sql =
            @"SELECT * FROM configs " +
            "WHERE id = @ConfigId";

        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<ConfigDto>(sql, group);

        return result.FirstOrDefault();
    }

    public async Task<ConfigDto> AddConfigAsync(ConfigDto config)
    {
        var sql =
            "INSERT INTO configs " +
            "OUTPUT INSERTED.* " +
            "VALUES(@NumberMealsPerDay, @StartFeeding, @EndFeeding)";
        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<ConfigDto>(sql, config);

        return result.FirstOrDefault();
    }

    public async Task DeleteConfigAsync(ConfigDto config)
    {
        var sql =
            @"DELETE groups " +
            "WHERE id = @Id";

        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.ExecuteAsync(sql, config);
    }

    public async Task UpdateConfigAsync(ConfigDto config)
    {
        var sql = "UPDATE configs " +
                  "SET number_meals_per_day = @NumberMealsPerDay, " +
                  "start_Feeding = @StartFeeding, " +
                  "end_feeding = @EndFeeding " +
                  "WHERE Id = @Id";

        var connection = await _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, config);
    }
}