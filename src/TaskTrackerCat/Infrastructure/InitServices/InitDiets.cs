using Dapper;
using Microsoft.Data.SqlClient;
using TaskTrackerCat.Infrastructure.Factories.Interfaces;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Infrastructure.InitServices;

public class InitDiets
{
    private readonly IDbConnectionFactory<SqlConnection> _dbConnectionFactory;
    private readonly IGroupRepository _groupRepository;
    private readonly IConfigRepository _configRepository;

    private List<DietDto> _diets;
    private GroupDto _group;
    private ConfigDto _config;

    private int countServingNumber = 1;
    private DateTime estimatedDateFeeding;
    private int daysInMonth;
    private TimeSpan INTERVAL;

    public InitDiets(IDbConnectionFactory<SqlConnection> dbConnectionFactory, IConfigRepository configRepository,
        IGroupRepository groupRepository)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _configRepository = configRepository;
        _groupRepository = groupRepository;
    }

    public async Task Init(GroupDto group)
    {
        _group = group;
        _diets = new List<DietDto>();
        await GetConfig();

        await InitMonth();
        _diets.Clear();
        await InitNextMonth();
    }

    private async Task GetConfig()
    {
        _config = await _configRepository.GetConfigFromGroupAsync(_group);
        SetInterval();
    }

    private void SetInterval()
    {
        var timeFeeding = _config.EndFeeding - _config.StartFeeding;
        var notRoundedInterval = timeFeeding / (_config.NumberMealsPerDay - 1);

        //Код взят с сайта. https://kkblog.ru/rounding-datetime-datestamp/
        //Округляем в меньшую сторону.
        var sec = notRoundedInterval.TotalSeconds;
        var divider = 5 * 60; // Делитель.
        //выравниваем секунды по началу интервала.
        var newSec = Math.Floor(sec / divider) * divider;
        //переводим секунды в такты.
        var newTicks = (long) newSec * 10000000;
        INTERVAL = new TimeSpan(newTicks);
    }

    private async Task InitMonth()
    {
        //Проверка на существование данных в таблице.
        var sql =
            "SELECT count(*) FROM diets " +
            "WHERE group_id = @Id";
        var connection = await _dbConnectionFactory.CreateConnection();

        var resultIsNotEmpty = await connection.QueryAsync<bool>(sql, _group);
        if (resultIsNotEmpty.FirstOrDefault())
        {
            return;
        }

        //Дата приема еды начинается с текущего месяца.
        estimatedDateFeeding = new DateTime(
            DateTime.Now.Year,
            DateTime.Now.Month,
            1,
            _config.StartFeeding.Hours,
            _config.StartFeeding.Minutes,
            _config.StartFeeding.Milliseconds);
        //Количество дней в текущем месяце.
        daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
        countServingNumber = 1;

        await AddDiets();
    }

    private async Task InitNextMonth()
    {
        //Проверка на существование будущего месяца.
        var sql =
            "SELECT MAX(estimated_date_feeding) FROM diets " +
            "WHERE group_id = @Id";
        var connection = await _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<DateTime>(sql, _group);

        var maxMonth = result.FirstOrDefault().Month;
        var nextMonth = DateTime.Now.AddMonths(1).Month;

        //Если максимальныая дата масяца совпадает с будущим месяцем.
        if (maxMonth == nextMonth)
        {
            return;
        }

        //Дата приема еды начинается со следующего месяца.
        estimatedDateFeeding = new DateTime(
                DateTime.Now.Year,
                DateTime.Now.Month,
                1,
                _config.StartFeeding.Hours,
                _config.StartFeeding.Minutes,
                _config.StartFeeding.Milliseconds)
            .AddMonths(1);
        //Дней в следующем месяце.
        daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.AddMonths(1).Month);
        countServingNumber = 1;

        await AddDiets();
    }

    private async Task AddDiets()
    {
        for (var i = 0; i < daysInMonth * _config.NumberMealsPerDay; i++)
        {
            AddDiet();
        }

        var sql =
            "INSERT INTO diets " +
            "(serving_number, status, estimated_date_feeding, group_id) " +
            "VALUES(@ServingNumber, @Status, @EstimatedDateFeeding, @GroupId)";
        var connection = await _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, _diets);
    }

    private void AddDiet()
    {
        var diet = new DietDto()
        {
            ServingNumber = countServingNumber,
            Status = false,
            EstimatedDateFeeding = estimatedDateFeeding,
            GroupId = _group.Id
        };

        if (countServingNumber == _config.NumberMealsPerDay)
        {
            SetEndTimeFeeding();
            diet.EstimatedDateFeeding = estimatedDateFeeding;
            _diets.Add(diet);

            SetStartTimeFeeding();
            estimatedDateFeeding = estimatedDateFeeding.AddDays(1); //Кормить каждый день.
            countServingNumber = 1;
            return;
        }

        estimatedDateFeeding = estimatedDateFeeding.AddHours(INTERVAL.Hours).AddMinutes(INTERVAL.Minutes);

        _diets.Add(diet);
        countServingNumber++;
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
}