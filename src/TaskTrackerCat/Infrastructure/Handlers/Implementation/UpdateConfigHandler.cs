using Dapper;
using Microsoft.Data.SqlClient;
using TaskTrackerCat.Infrastructure.Factories.Interfaces;
using TaskTrackerCat.Infrastructure.Handlers.Commands;
using TaskTrackerCat.Infrastructure.Handlers.Interfaces;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Infrastructure.Handlers.Implementation;

public class UpdateConfigHandler : IRequestHandler<UpdateConfigCommand>
{
    #region Fields

    private readonly IDbConnectionFactory<SqlConnection> _dbConnectionFactory;
    private readonly IConfigRepository _configRepository;
    private ConfigDto _config;
    private GroupDto _group;

    private int NUMBER_MEALS_PER_DAY;
    private TimeSpan INTERVAL;

    private int countServingNumber;
    private DateTime estimatedDateFeeding;

    #endregion

    public UpdateConfigHandler(IDbConnectionFactory<SqlConnection> dbConnectionFactory,
        IConfigRepository configRepository)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _configRepository = configRepository;
    }

    /// <summary>
    /// Обновляет количество приемов еды в день.
    /// </summary>
    /// <param name="request"></param>
    public async Task Handle(UpdateConfigCommand request)
    {
        _config = new ConfigDto()
        {
            Id = request.Group.ConfigId,
            NumberMealsPerDay = request.Model.NumberMealsPerDay,
            StartFeeding = new TimeSpan(request.Model.StartFeeding.Hour, request.Model.StartFeeding.Minute,
                request.Model.StartFeeding.Second),
            EndFeeding = new TimeSpan(request.Model.EndFeeding.Hour, request.Model.EndFeeding.Minute,
                request.Model.EndFeeding.Second)
        };
        _group = request.Group;

        //Получение конфига перед обновлением.  
        var pastConfig = await _configRepository.GetConfigAsync(_config);

        if (pastConfig.NumberMealsPerDay == _config.NumberMealsPerDay &&
            pastConfig.StartFeeding == _config.StartFeeding &&
            pastConfig.EndFeeding == _config.EndFeeding)
        {
            return;
        }

        await _configRepository.UpdateConfigAsync(_config);
        NUMBER_MEALS_PER_DAY = _config.NumberMealsPerDay;

        try
        {
            if (pastConfig.NumberMealsPerDay != _config.NumberMealsPerDay)
            {
                await UpdateDiets(pastConfig);
            }

            await UpdateDateFeeding();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await _configRepository.UpdateConfigAsync(pastConfig);
            throw;
        }
    }

    private async Task UpdateDiets(ConfigDto pastConfig)
    {
        if (_config.NumberMealsPerDay < pastConfig.NumberMealsPerDay)
        {
            await DeleteDiets();
            return;
        }

        await AddDiets(pastConfig.NumberMealsPerDay);
    }

    /// <summary>
    /// Удаляет приемы еды, которые больше нового значения приемов еды в день.
    /// </summary>
    private async Task DeleteDiets()
    {
        var firstDayInCurrentMonth = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1);

        //Удаляем все приемы пищи с текущего месяца.
        var sqlDiets = @"DELETE diets " +
                       "WHERE serving_number > @NUMBER_MEALS_PER_DAY " +
                       "AND estimated_date_feeding > @firstDayInCurrentMonth " +
                       "AND group_id = @Id";

        var connection = await _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sqlDiets, new {NUMBER_MEALS_PER_DAY, firstDayInCurrentMonth, _group.Id});
    }

    /// <summary>
    /// Добавляет новые приемы пищи в текущий и будущий месяц.
    /// Стоит отметить что будущий месяц всего один - следущий от текущего, поэтому запрос на получение максимального месяца не пишется.
    /// </summary>
    /// <param name="pastNumberMealsPerDay">Количество примемов еды в день до изменения.</param>
    private async Task AddDiets(int pastNumberMealsPerDay)
    {
        //Редактирование даты приема еды начинается с текущего месяца.
        //Устанавливаем максимальное значение, так как при изменении даты кормления идет сортировка по дате.
        //Если дату не устанавить в максимальное значение то добавленые примемы будут первыми.
        estimatedDateFeeding = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1,
            _config.EndFeeding.Hours,
            _config.EndFeeding.Minutes,
            _config.EndFeeding.Milliseconds);
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
            "(serving_number, status, estimated_date_feeding, group_id) " +
            "VALUES(@ServingNumber, @Status, @EstimatedDateFeeding, @GroupId)";

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
        var daysInCurrentMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
        var daysInNextMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.AddMonths(1).Month);

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
            EstimatedDateFeeding = estimatedDateFeeding,
            GroupId = _group.Id
        };

        if (countServingNumber == NUMBER_MEALS_PER_DAY)
        {
            countServingNumber = pastNumberMealsPerDay;
            estimatedDateFeeding = estimatedDateFeeding.AddDays(1); //Кормить каждый день.
        }

        diets.Add(diet);
    }

    private async Task UpdateDateFeeding()
    {
        SetIntervalFeeding();

        //Редактирование даты приема еды начинается с текущего месяца.
        var dateFeeding = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        var sqlGet =
            @"SELECT id, serving_number FROM diets " +
            "WHERE estimated_date_feeding >= @dateFeeding " +
            "AND group_id = @Id " +
            "ORDER BY estimated_date_feeding, serving_number";

        var connection = await _dbConnectionFactory.CreateConnection();
        var diets = await connection.QueryAsync<DietDto>(sqlGet, new {dateFeeding, _group.Id});

        AddDateFeeding(diets);

        var sqlUp = "UPDATE diets " +
                    "SET estimated_date_feeding = @EstimatedDateFeeding " +
                    "WHERE Id = @Id";

        await connection.ExecuteAsync(sqlUp, diets);
    }

    private void AddDateFeeding(IEnumerable<DietDto> diets)
    {
        estimatedDateFeeding = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1,
            _config.StartFeeding.Hours,
            _config.StartFeeding.Minutes,
            _config.StartFeeding.Milliseconds);

        //Число порции начинается с первой.
        countServingNumber = 1;

        foreach (var diet in diets)
        {
            if (countServingNumber == NUMBER_MEALS_PER_DAY)
            {
                SetEndTimeFeeding();
                diet.EstimatedDateFeeding = estimatedDateFeeding;

                SetStartTimeFeeding();
                estimatedDateFeeding = estimatedDateFeeding.AddDays(1); //Кормить каждый день.
                countServingNumber = 1;
                continue;
            }

            diet.EstimatedDateFeeding = estimatedDateFeeding;
            estimatedDateFeeding = estimatedDateFeeding.AddHours(INTERVAL.Hours).AddMinutes(INTERVAL.Minutes);
            countServingNumber++;
        }
    }

    private void SetStartTimeFeeding()
    {
        //Устанавливаем значения часа и минут в начальные значения.
        estimatedDateFeeding = new DateTime(
            estimatedDateFeeding.Year,
            estimatedDateFeeding.Month,
            estimatedDateFeeding.Day,
            _config.StartFeeding.Hours,
            _config.StartFeeding.Minutes,
            estimatedDateFeeding.Millisecond);
    }

    private void SetEndTimeFeeding()
    {
        estimatedDateFeeding = new DateTime(
            estimatedDateFeeding.Year,
            estimatedDateFeeding.Month,
            estimatedDateFeeding.Day,
            _config.EndFeeding.Hours,
            _config.EndFeeding.Minutes,
            estimatedDateFeeding.Millisecond);
    }

    private void SetIntervalFeeding()
    {
        var numberMealsPerDay = _config.NumberMealsPerDay;
        var timeFeeding = _config.EndFeeding - _config.StartFeeding;
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