using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Npgsql;
using TaskTrackerCat.Infrastructure.Factories;
using TaskTrackerCat.Infrastructure.Factories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Infrastructure;

public class InitService
{
    private readonly IServiceProvider _serviceProvider;
    private IDbConnectionFactory<SqlConnection> _dbConnectionFactory;

    private readonly List<DietDto> _Diets;

    private int numberMealsPerDay = 3;
    private int countServingNumber = 1;
    private DateTime estimatedDateFeeding;
    private int daysInMonth;

    public InitService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _Diets = new List<DietDto>();
        estimatedDateFeeding = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
    }

    public async Task Init()
    {
        await InitMonth();
        _Diets.Clear();
        await InitNextMonth();
    }

    private async Task InitMonth()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbConnectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory<SqlConnection>>();
        var connection = await dbConnectionFactory.CreateConnection();

        //Приверка на существование данных в таблице.
        var sql = @"SELECT count(*) FROM diets";
        var resultIsEmpty = await connection.QueryAsync<bool>(sql);
        if (resultIsEmpty.FirstOrDefault())
        {
            return;
        }

        //Указываем что дата приема еды начинается с текущего месяца.
        estimatedDateFeeding = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        //Записываем количество дней в текущем месяце.
        daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
        await AddDiets(connection);
    }

    private async Task InitNextMonth()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbConnectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory<SqlConnection>>();
        var connection = await dbConnectionFactory.CreateConnection();

        //Проверка на существование будущего месяца.
        var sql = @"SELECT MAX(estimated_date_feeding) FROM diets";
        var result = await connection.QueryAsync<DateTime>(sql);

        var maxMonth = result.FirstOrDefault().Month;
        var nextMonth = DateTime.Now.AddMonths(1).Month;

        //Если максимальныая дата масяца совпадает с будущим месяцем.
        if (maxMonth == nextMonth)
        {
            return;
        }

        //Указываем что дата приема еды начинается со следующего месяца.
        estimatedDateFeeding = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
        //Записываем количество дней в следующем месяце.
        daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.AddMonths(1).Month);
        await AddDiets(connection);
    }

    private async Task AddDiets(SqlConnection сonnection)
    {
        var sql =
            "INSERT INTO diets " +
            "(serving_number, status, estimated_date_feeding) " +
            "VALUES(@ServingNumber, @Status, @EstimatedDateFeeding)";

        for (var i = 0; i < daysInMonth * numberMealsPerDay; i++)
        {
            AddDiet();
        }

        await сonnection.ExecuteAsync(sql, _Diets);
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