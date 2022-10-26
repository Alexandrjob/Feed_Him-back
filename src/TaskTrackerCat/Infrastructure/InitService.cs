using System.Data;
using Dapper;
using Npgsql;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Infrastructure;

public class InitService
{
    private readonly string _connectionString;
    private readonly List<DietDto> _Diets;

    private int numberMealsPerDay = 3;
    private int countServingNumber = 1;
    private DateTime estimatedDateFeeding;

    public InitService(IConfiguration configuration)
    {
        _Diets = new List<DietDto>();
        _connectionString = configuration.GetSection("ConnectionString").Value;
        estimatedDateFeeding = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
    }

    public async Task Init()
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"SELECT MAX(estimated_date_feeding) FROM diets";

        var result = await connection.QueryAsync<DateTime>(sql);

        var maxMonth = result.FirstOrDefault().Month;
        var nextMonth = DateTime.Now.AddMonths(1).Month;

        //Если максимальныая дата масяца совпадает с будущим месяцем.
        if (maxMonth == nextMonth)
        {
            return;
        }

        await InitNextMonth(connection);
    }

    private async Task InitNextMonth(IDbConnection connection)
    {
        var sql =
            "INSERT INTO diets " +
            "(serving_number, status, estimated_date_feeding) " +
            "VALUES(@ServingNumber, @Status, @EstimatedDateFeeding)";
        var daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

        for (var i = 0; i < daysInMonth * numberMealsPerDay - 3; i++)
        {
            AddDiet();
        }

        await connection.ExecuteAsync(sql, _Diets);
    }

    private void AddDiet()
    {
        var diet = new DietDto()
        {
            ServingNumber = countServingNumber,
            Status = false,
            EstimatedDateFeeding = estimatedDateFeeding
        };

        if (countServingNumber == numberMealsPerDay)
        {
            countServingNumber = 0;
            estimatedDateFeeding = estimatedDateFeeding.AddDays(1); //Кормить каждый день.
        }

        _Diets.Add(diet);
        countServingNumber++;
    }
}