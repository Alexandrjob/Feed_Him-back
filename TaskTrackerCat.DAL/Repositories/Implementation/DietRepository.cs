using Dapper;
using Microsoft.Data.SqlClient;
using TaskTrackerCat.DAL.Factories.Interfaces;
using TaskTrackerCat.DAL.Models;
using TaskTrackerCat.DAL.Repositories.Interfaces;

namespace TaskTrackerCat.DAL.Repositories.Implementation;

public class DietRepository : IDietRepository
{
    private readonly IDbConnectionFactory<SqlConnection> _dbConnectionFactory;

    public DietRepository(IDbConnectionFactory<SqlConnection> dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public async Task<List<DietDto>> GetAsync()
    {
        var firstDayMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var lastDayMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1);

        var sql =
            @"SELECT * FROM diets " +
            "WHERE estimated_date_feeding >= @firstDayMonth " +
            "AND estimated_date_feeding <= @lastDayMonth " +
            "ORDER BY estimated_date_feeding, serving_number";

        var connection = _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<DietDto>(sql, new {firstDayMonth, lastDayMonth});

        return result.ToList();
    }

    public async Task<List<DietDto>> GetAsync(DateTime dateFeeding)
    {
        var sqlGet =
            @"SELECT id, serving_number FROM diets " +
            "WHERE estimated_date_feeding >= @dateFeeding " +
            "ORDER BY estimated_date_feeding, serving_number";

        var connection = _dbConnectionFactory.CreateConnection();
        var diets = await connection.QueryAsync<DietDto>(sqlGet, new {dateFeeding});
        return diets.ToList();
    }

    public async Task<DietDto> UpdateAsync(DietDto diet)
    {
        var sql = "UPDATE diets " +
                  "SET waiter_name = @WaiterName, " +
                  "date = @Date, " +
                  "status = @Status " +
                  "OUTPUT inserted.* " +
                  "WHERE Id = @Id";

        var connection = _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<DietDto>(sql, diet);
        return result.FirstOrDefault();
    }

    public async Task UpdateAsync(List<DietDto> diets)
    {
        var sql = "UPDATE diets " +
                  "SET estimated_date_feeding = @EstimatedDateFeeding " +
                  "WHERE Id = @Id";

        var connection = _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, diets);
    }

    public async Task AddAsync(List<DietDto> diets)
    {
        var sql =
            "INSERT INTO diets " +
            "(serving_number, status, estimated_date_feeding) " +
            "VALUES(@ServingNumber, @Status, @EstimatedDateFeeding)";

        var connection = _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, diets);
    }

    public async Task DeleteDietsAsync(int numberMealsPerDay, DateTime firstDayInCurrentMonth)
    {
        //Удаляем все приемы пищи с текущего месяца.
        var sql = @"DELETE diets " +
                       "WHERE serving_number > @numberMealsPerDay " +
                       "AND estimated_date_feeding >= @firstDayInCurrentMonth";

        var connection = _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new {numberMealsPerDay, firstDayInCurrentMonth});
    }
}