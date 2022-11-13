using Dapper;
using Microsoft.Data.SqlClient;
using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Infrastructure.Factories.Interfaces;
using TaskTrackerCat.Infrastructure.Handlers.Interfaces;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Infrastructure.Handlers;

public class UpdateConfigHadler : IRequestHandler<ConfigViewModel>
{
    #region Fields

    private readonly IDbConnectionFactory<SqlConnection> _dbConnectionFactory;
    private readonly IDietRepository _dietRepository;
    private readonly IConfigRepository _configRepository;

    private int NUMBER_MEALS_PER_DAY;
    private TimeSpan INTERVAL;

    private int countServingNumber;
    private DateTime estimatedDateFeeding;

    #endregion

    public UpdateConfigHadler(IDbConnectionFactory<SqlConnection> dbConnectionFactory,
        IDietRepository dietRepository,
        IConfigRepository configRepository)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _dietRepository = dietRepository;
        _configRepository = configRepository;
    }

    /// <summary>
    /// Обновляет количество приемов еды в день.
    /// </summary>
    /// <param name="model"></param>
    public async Task Handle(ConfigViewModel model)
    {
        var newConfig = new ConfigDto()
        {
            Id = model.Id,
            NumberMealsPerDay = model.NumberMealsPerDay,
            StartFeeding = new TimeSpan(model.StartFeeding.Hour, model.StartFeeding.Minute, model.StartFeeding.Second),
            EndFeeding = new TimeSpan(model.EndFeeding.Hour, model.EndFeeding.Minute, model.EndFeeding.Second)
        };

        //Получение конфига перед обновлением.  
        ConfigDto pastConfig = await _configRepository.GetConfigAsync(newConfig);

        if (pastConfig.NumberMealsPerDay == newConfig.NumberMealsPerDay &&
            pastConfig.StartFeeding == newConfig.StartFeeding &&
            pastConfig.EndFeeding == newConfig.EndFeeding)
        {
            return;
        }

        await _configRepository.UpdateConfigAsync(newConfig);
        NUMBER_MEALS_PER_DAY = newConfig.NumberMealsPerDay;

        try
        {
            if (pastConfig.NumberMealsPerDay != newConfig.NumberMealsPerDay)
            {
                await UpdateDiets(newConfig, pastConfig);
            }

            await UpdateDateFeeding(newConfig);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await _configRepository.UpdateConfigAsync(pastConfig);
            throw;
        }
    }

    private async Task UpdateDiets(ConfigDto newConfig, ConfigDto pastConfig)
    {
        if (newConfig.NumberMealsPerDay < pastConfig.NumberMealsPerDay)
        {
            await DeleteDiets();
            return;
        }

        await AddDiets(newConfig, pastConfig.NumberMealsPerDay);
    }

    /// <summary>
    /// Удаляет приемы еды, которые больше нового значения приемов еды в день.
    /// </summary>
    private async Task DeleteDiets()
    {
        //Удаляем все приемы пищи с текущего месяца.
        var sqlDiets = @"DELETE diets " +
                       "WHERE serving_number > @NUMBER_MEALS_PER_DAY";

        var connection = await _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sqlDiets, new {NUMBER_MEALS_PER_DAY});
    }

    /// <summary>
    /// Добавляет новые приемы пищи в текущий и будущий месяц.
    /// Стоит отметить что будущий месяц всего один - следущий от текущего, поэтому запрос на получение максимального месяца не пишется.
    /// </summary>
    /// <param name="config">Конфигурация приемов еды.</param>
    /// <param name="pastNumberMealsPerDay">Количество примемов еды в день до изменения.</param>
    private async Task AddDiets(ConfigDto config, int pastNumberMealsPerDay)
    {
        //Редактирование даты приема еды начинается с текущего месяца.
        //Устанавливаем максимальное значение, так как при изменении даты кормления идет сортировка по дате.
        //Если дату не устанавить в максимальное значение то добавленые примемы будут первыми.
        estimatedDateFeeding = new DateTime(
            DateTime.Now.Year,
            DateTime.Now.Month,
            1,
            config.EndFeeding.Hours,
            config.EndFeeding.Minutes,
            config.EndFeeding.Milliseconds);
        //Число порции начинается с последней прошлой.
        countServingNumber = pastNumberMealsPerDay;
        var numberDiets = GetTotalNumberDiets(pastNumberMealsPerDay);

        var diets = new List<DietDto>();
        for (var i = 0; i < numberDiets; i++)
        {
            AddDiet(diets, pastNumberMealsPerDay);
        }

        var sql =
            "INSERT INTO diets " +
            "(serving_number, status, estimated_date_feeding) " +
            "VALUES(@ServingNumber, @Status, @EstimatedDateFeeding)";

        var connection = await _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, diets);
    }

    /// <summary>
    /// Высчитывает количество приемов еды с первого текущего месяца по следующий.
    /// </summary>
    /// <param name="pastNumberMealsPerDay"></param>
    /// <returns>Количесво приемов еды.</returns>
    private int GetTotalNumberDiets(int pastNumberMealsPerDay)
    {
        var daysInCurrentMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
        var daysInNextMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.AddMonths(1).Month);

        return (daysInNextMonth + daysInCurrentMonth) * (NUMBER_MEALS_PER_DAY - pastNumberMealsPerDay);
    }

    /// <summary>
    /// Добавляет один прием еды в список.
    /// </summary>
    /// <param name="diets">список приемов еды.</param>
    /// <param name="pastNumberMealsPerDay">Значение количества примемов еды в день до изменения.</param>
    private void AddDiet(List<DietDto> diets, int pastNumberMealsPerDay)
    {
        countServingNumber++;
        var diet = new DietDto()
        {
            ServingNumber = countServingNumber,
            Status = false,
            EstimatedDateFeeding = estimatedDateFeeding
        };

        if (countServingNumber == NUMBER_MEALS_PER_DAY)
        {
            countServingNumber = pastNumberMealsPerDay;
            estimatedDateFeeding = estimatedDateFeeding.AddDays(1); //Кормить каждый день.
        }

        diets.Add(diet);
    }

    private async Task UpdateDateFeeding(ConfigDto config)
    {
        SetIntervalFeeding(config);

        //Редактирование даты приема еды начинается с текущего месяца.
        var dateFeeding = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        var sqlGet =
            @"SELECT id, serving_number FROM diets " +
            "WHERE estimated_date_feeding >= @dateFeeding " +
            "ORDER BY estimated_date_feeding, serving_number";

        var connection = await _dbConnectionFactory.CreateConnection();
        var diets = await connection.QueryAsync<DietDto>(sqlGet, new {dateFeeding});

        AddDateFeeding(config, diets);

        var sqlUp = "UPDATE diets " +
                    "SET estimated_date_feeding = @EstimatedDateFeeding " +
                    "WHERE Id = @Id";

        await connection.ExecuteAsync(sqlUp, diets);
    }

    private void AddDateFeeding(ConfigDto newConfig, IEnumerable<DietDto> diets)
    {
        estimatedDateFeeding = new DateTime(
            DateTime.Now.Year,
            DateTime.Now.Month,
            1,
            newConfig.StartFeeding.Hours,
            newConfig.StartFeeding.Minutes,
            newConfig.StartFeeding.Milliseconds);

        //Число порции начинается с первой.
        countServingNumber = 1;

        foreach (var diet in diets)
        {
            if (countServingNumber == NUMBER_MEALS_PER_DAY)
            {
                SetEndTimeFeeding(newConfig);
                diet.EstimatedDateFeeding = estimatedDateFeeding;

                SetStartTimeFeeding(newConfig);
                estimatedDateFeeding = estimatedDateFeeding.AddDays(1); //Кормить каждый день.
                countServingNumber = 1;
                continue;
            }

            diet.EstimatedDateFeeding = estimatedDateFeeding;
            estimatedDateFeeding = estimatedDateFeeding.AddHours(INTERVAL.Hours).AddMinutes(INTERVAL.Minutes);
            countServingNumber++;
        }
    }

    private void SetStartTimeFeeding(ConfigDto newConfig)
    {
        //Устанавливаем значения часа и минут в начальные значения.
        estimatedDateFeeding = new DateTime(
            estimatedDateFeeding.Year,
            estimatedDateFeeding.Month,
            estimatedDateFeeding.Day,
            newConfig.StartFeeding.Hours,
            newConfig.StartFeeding.Minutes,
            estimatedDateFeeding.Millisecond);
    }

    private void SetEndTimeFeeding(ConfigDto newConfig)
    {
        estimatedDateFeeding = new DateTime(
            estimatedDateFeeding.Year,
            estimatedDateFeeding.Month,
            estimatedDateFeeding.Day,
            newConfig.EndFeeding.Hours,
            newConfig.EndFeeding.Minutes,
            estimatedDateFeeding.Millisecond);
    }

    private void SetIntervalFeeding(ConfigDto config)
    {
        var numberMealsPerDay = config.NumberMealsPerDay;
        var timeFeeding = config.EndFeeding - config.StartFeeding;
        //Вычитание происходит из-за того, что последний прием записывается от конфига.
        var notRoundedInterval = timeFeeding / (numberMealsPerDay - 1);

        //Код взят с сайта. https://kkblog.ru/rounding-datetime-datestamp/
        //Округляем в меньшую сторону.
        var sec = notRoundedInterval.TotalSeconds;
        var divider = 5 * 60;
        //выравниваем секунды по началу интервала.
        var newSec = Math.Floor(sec / divider) * divider;
        //переводим секунды в такты.
        var newTicks = (long) newSec * 10000000;
        INTERVAL = new TimeSpan(newTicks);
    }
}