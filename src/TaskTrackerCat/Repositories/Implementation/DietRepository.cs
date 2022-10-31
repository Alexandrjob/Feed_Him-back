using Dapper;
using Microsoft.Data.SqlClient;
using Npgsql;
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
        var daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

        var firstDayMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var lastDayMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, daysInMonth);
        var sql =
            @"SELECT * FROM diets " +
            "WHERE estimated_date_feeding >= @firstDayMonth " +
            "AND estimated_date_feeding <= @lastDayMonth " +
            "ORDER BY estimated_date_feeding";

        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<DietDto>(sql, new {firstDayMonth, lastDayMonth});

        return result.ToList();
    }

    public async Task UpdateAsync(DietDto diet)
    {
        var sql = "UPDATE diets " +
                  "SET serving_number = @ServingNumber, " +
                  "waiter_name = @WaiterName, " +
                  "date = @Date, " +
                  "status = @Status " +
                  "WHERE Id = @Id";

        var connection = await _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, diet);
    }
}