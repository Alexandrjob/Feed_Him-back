using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Npgsql;
using TaskTrackerCat.Infrastructure.Factories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Infrastructure;

public class InitService
{
    private readonly IServiceProvider _serviceProvider;
    //private IDbConnectionFactory<SqlConnection> _dbConnectionFactory;

    private SqlConnection _connection ;
    private readonly List<DietDto> _diets;

    private int NUMBER_MEALS_PER_DAY;
    private int countServingNumber = 1;
    private DateTime estimatedDateFeeding;
    private int daysInMonth;

    public InitService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _diets = new List<DietDto>();
        estimatedDateFeeding = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
    }

    public async Task Init()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbConnectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory<SqlConnection>>();
        _connection = await dbConnectionFactory.CreateConnection();
        
        await InitConfig();
        await InitMonth();
        _diets.Clear();
        await InitNextMonth();
    }

    private async Task InitConfig()
    {
        //Проверка на существование данных в таблице.
        var sqlCheck = @"SELECT count(*) FROM config";
        var resultIsEmpty = await _connection.QueryAsync<bool>(sqlCheck);
        if (resultIsEmpty.FirstOrDefault())
        {
            return;
        }

        var config = new ConfigDto()
        {
            NumberMealsPerDay = 7
        };
        NUMBER_MEALS_PER_DAY = config.NumberMealsPerDay;
        
        var sqlInsert =
            "INSERT INTO config " +
            "(number_meals_per_day) " +
            "VALUES(@NumberMealsPerDay)";
        await _connection.ExecuteAsync(sqlInsert, config);
    }

    private async Task InitMonth()
    {
        //Проверка на существование данных в таблице.
        var sql = @"SELECT count(*) FROM diets";
        var resultIsEmpty = await _connection.QueryAsync<bool>(sql);
        if (resultIsEmpty.FirstOrDefault())
        {
            return;
        }

        //Дата приема еды начинается с текущего месяца.
        estimatedDateFeeding = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        //Количество дней в текущем месяце.
        daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

        await AddDiets();
    }

    private async Task InitNextMonth()
    {
        //Проверка на существование будущего месяца.
        var sql = @"SELECT MAX(estimated_date_feeding) FROM diets";
        var result = await _connection.QueryAsync<DateTime>(sql);

        var maxMonth = result.FirstOrDefault().Month;
        var nextMonth = DateTime.Now.AddMonths(1).Month;

        //Если максимальныая дата масяца совпадает с будущим месяцем.
        if (maxMonth == nextMonth)
        {
            return;
        }

        //Дата приема еды начинается со следующего месяца.
        estimatedDateFeeding = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
        //Дней в следующем месяце.
        daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.AddMonths(1).Month);

        await AddDiets();
    }

    private async Task AddDiets()
    {
        var sql =
            "INSERT INTO diets " +
            "(serving_number, status, estimated_date_feeding) " +
            "VALUES(@ServingNumber, @Status, @EstimatedDateFeeding)";
        
        
        for (var i = 0; i < daysInMonth * NUMBER_MEALS_PER_DAY; i++)
        {
            AddDiet();
        }

        await _connection.ExecuteAsync(sql, _diets);
    }

    private void AddDiet()
    {
        var diet = new DietDto()
        {
            ServingNumber = countServingNumber,
            Status = false,
            EstimatedDateFeeding = estimatedDateFeeding
        };

        if (countServingNumber == NUMBER_MEALS_PER_DAY)
        {
            countServingNumber = 0;
            estimatedDateFeeding = estimatedDateFeeding.AddDays(1); //Кормить каждый день.
        }

        _diets.Add(diet);
        countServingNumber++;
    }
}