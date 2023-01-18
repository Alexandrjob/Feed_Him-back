using Dapper;
using Microsoft.Data.SqlClient;
using TaskTrackerCat.DAL.Factories.Interfaces;
using TaskTrackerCat.DAL.Models;
using TaskTrackerCat.DAL.Repositories.Interfaces;

namespace TaskTrackerCat.DAL.Repositories.Implementation;

public class ConfigRepository : IConfigRepository
{
    private readonly IDbConnectionFactory<SqlConnection> _dbConnectionFactory;

    public ConfigRepository(IDbConnectionFactory<SqlConnection> dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public async Task AddAsync(ConfigDto config)
    {
        var sql =
            "INSERT INTO config " +
            "(number_meals_per_day, start_feeding, end_feeding) " +
            "VALUES(@NumberMealsPerDay, @StartFeeding, @EndFeeding)";

        var connection = _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, config);
    }

    public async Task<ConfigDto> GetAsync(ConfigDto config)
    {
        var sql = @"SELECT * FROM config";

        var connection = _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<ConfigDto>(sql, config);

        return result.FirstOrDefault();
    }

    public async Task UpdateAsync(ConfigDto config)
    {
        var sql = "UPDATE config " +
                  "SET number_meals_per_day = @NumberMealsPerDay, " +
                  "start_Feeding = @StartFeeding, " +
                  "end_feeding = @EndFeeding " +
                  "WHERE Id = @Id";

        var connection = _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, config);
    }
}