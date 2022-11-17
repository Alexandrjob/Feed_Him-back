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

    public async Task<List<DietDto>> GetDietsAsync(int groupId)
    {
        var daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

        var firstDayMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var lastDayMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, daysInMonth);
        var sql =
            @"SELECT * FROM diets " +
            "WHERE estimated_date_feeding >= @firstDayMonth " +
            "AND estimated_date_feeding <= @lastDayMonth " +
            "AND group_id = @groupId " +
            "ORDER BY estimated_date_feeding, serving_number";

        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<DietDto>(sql, new {firstDayMonth, lastDayMonth, groupId});

        return result.ToList();
    }

    public async Task UpdateDietAsync(DietDto diet)
    {
        var sql = "UPDATE diets " +
                  "SET waiter_name = @WaiterName, " +
                  "date = @Date, " +
                  "status = @Status " +
                  "WHERE Id = @Id";

        var connection = await _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, diet);
    }

    public async Task<DietDto> GetDietAsync(DietDto diet)
    {
        var sql =
            @"SELECT group_id FROM diets " +
            "WHERE id = @Id";

        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<DietDto>(sql, diet);

        return result.FirstOrDefault();
    }

    public async Task DeleteDietsAsync(GroupDto group)
    {
        var sql =
            "DELETE diets " +
            "WHERE group_id = @Id";

        var connection = await _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, group);
    }
}