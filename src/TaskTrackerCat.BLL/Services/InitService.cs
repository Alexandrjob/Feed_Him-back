using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskTrackerCat.BLL.Services.Helpers;
using TaskTrackerCat.DAL.Factories.Interfaces;
using TaskTrackerCat.DAL.Models;
using TaskTrackerCat.DAL.Repositories.Interfaces;

namespace TaskTrackerCat.BLL.Services;

public class InitService
{
    #region Fields

    private readonly ILogger<InitService> _logger;
    private readonly IServiceProvider _serviceProvider;

    private ConfigHelper _configHelper;

    private IConfigRepository _configRepository;
    private IDietRepository _dietRepository;

    private SqlConnection _connection;
    private readonly List<DietDto> _diets;

    public ConfigDto Config { get; set; }

    private int countServingNumber = 1;
    private DateTime estimatedDateFeeding;
    private int daysInMonth;
    private TimeSpan INTERVAL;

    #endregion
    
    public InitService(ILogger<InitService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;


        _diets = new List<DietDto>();
    }

    public async Task Init()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbConnectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory<SqlConnection>>();
        _connection = dbConnectionFactory.CreateConnection();
        
        _configHelper = scope.ServiceProvider.GetRequiredService<ConfigHelper>();
        _configRepository = scope.ServiceProvider.GetRequiredService<IConfigRepository>();
        _dietRepository = scope.ServiceProvider.GetRequiredService<IDietRepository>();

        await InitConfig();
        await InitMonth();
        _diets.Clear();
        await InitNextMonth();
    }

    private async Task InitConfig()
    {
        Config = new ConfigDto
        {
            NumberMealsPerDay = 2,
            StartFeeding = new TimeSpan(7, 30, 0),
            EndFeeding = new TimeSpan(23, 00, 0)
        };

        INTERVAL = _configHelper.GetIntervalFeeding(Config);

        //Проверка на существование данных в таблице.
        var sql = @"SELECT * FROM config";
        var result = await _connection.QueryAsync<ConfigDto>(sql);
        var config = result.FirstOrDefault();
        if (config != null)
        {
            _logger.LogInformation("Конфигурация найдена в базе данных.");
            Config = config;
            INTERVAL = _configHelper.GetIntervalFeeding(Config);
            return;
        }

        try
        {
            await _configRepository.AddAsync(Config);
            _logger.LogInformation("Конфигурация записана в базу данных.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Ошибка отправки данных(config) в базу данных.");
            throw;
        }
    }

    private async Task InitMonth()
    {
        countServingNumber = 1;

        //Проверка на существование данных в таблице.
        var sql = @"SELECT count(*) FROM diets";
        var resultIsEmpty = await _connection.QueryAsync<bool>(sql);
        if (resultIsEmpty.FirstOrDefault())
        {
            _logger.LogInformation("Приемы пищи текущего месяца найдены в базе данных.");
            return;
        }

        //Дата приема еды начинается с текущего месяца.
        estimatedDateFeeding = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1,
            Config.StartFeeding.Hours,
            Config.StartFeeding.Minutes,
            Config.StartFeeding.Milliseconds);
        //Количество дней в текущем месяце.
        daysInMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);

        await AddDiets();
        _logger.LogInformation("Приемы пищи текущего месяца({Date}) записаны в базу данных.", DateTime.UtcNow);
    }

    private async Task InitNextMonth()
    {
        countServingNumber = 1;

        //Проверка на существование будущего месяца.
        var sql = @"SELECT MAX(estimated_date_feeding) FROM diets";
        var result = await _connection.QueryAsync<DateTime>(sql);

        var maxMonth = result.FirstOrDefault().Month;
        var nextMonth = DateTime.UtcNow.AddMonths(1).Month;

        //Если максимальныая дата масяца совпадает с будущим месяцем.
        if (maxMonth == nextMonth)
        {
            _logger.LogInformation("Приемы пищи следующего месяца найдены в базе данных.");
            return;
        }

        //Дата приема еды начинается со следующего месяца.
        estimatedDateFeeding = new DateTime(
                DateTime.UtcNow.Year,
                DateTime.UtcNow.Month,
                1,
                Config.StartFeeding.Hours,
                Config.StartFeeding.Minutes,
                Config.StartFeeding.Milliseconds)
            .AddMonths(1);
        //Дней в следующем месяце.
        daysInMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.AddMonths(1).Month);

        await AddDiets();
        _logger.LogInformation("Приемы пищи следующего месяца({Date}) записаны в базу данных.",
            DateTime.UtcNow.AddMonths(1));
    }

    private async Task AddDiets()
    {
        for (var i = 0; i < daysInMonth * Config.NumberMealsPerDay; i++) AddDiet();

        try
        {
            await _dietRepository.AddAsync(_diets);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Ошибка отправки данных(diets) в базу данных.");
            throw;
        }
    }

    private void AddDiet()
    {
        var diet = new DietDto
        {
            ServingNumber = countServingNumber,
            Status = false,
            EstimatedDateFeeding = estimatedDateFeeding
        };

        if (countServingNumber == Config.NumberMealsPerDay)
        {
            estimatedDateFeeding = _configHelper.GetEndTimeFeeding(estimatedDateFeeding, Config);
            diet.EstimatedDateFeeding = estimatedDateFeeding;
            _diets.Add(diet);

            estimatedDateFeeding = _configHelper.GetStartTimeFeeding(estimatedDateFeeding, Config);
            estimatedDateFeeding = estimatedDateFeeding.AddDays(1); //Кормить каждый день.
            countServingNumber = 1;
            return;
        }

        estimatedDateFeeding = estimatedDateFeeding.AddHours(INTERVAL.Hours).AddMinutes(INTERVAL.Minutes);

        _diets.Add(diet);
        countServingNumber++;
    }
}