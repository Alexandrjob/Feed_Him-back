using Dapper;
using Microsoft.Data.SqlClient;
using TaskTrackerCat.Infrastructure.Factories.Interfaces;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Repositories.Implementation;

public class DietRepository : IDietRepository
{
    private readonly IDbConnectionFactory<SqlConnection> _dbConnectionFactory;

    public DietRepository(IDbConnectionFactory<SqlConnection> dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public async Task<List<DietDto>> GetDietsAsync()
    {
        var firstDayMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var lastDayMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1);

        var sql =
            @"SELECT * FROM diets " +
            "WHERE estimated_date_feeding >= @firstDayMonth " +
            "AND estimated_date_feeding <= @lastDayMonth " +
            "ORDER BY estimated_date_feeding, serving_number";

        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<DietDto>(sql, new {firstDayMonth, lastDayMonth});

        return result.ToList();
    }

    public async Task<DietDto> UpdateDietAsync(DietDto diet)
    {
        var sql = "UPDATE diets " +
                  "SET waiter_name = @WaiterName, " +
                  "date = @Date, " +
                  "status = @Status " +
                  "OUTPUT inserted.* " +
                  "WHERE Id = @Id";

        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<DietDto>(sql, diet);
        return result.FirstOrDefault();
    }
}