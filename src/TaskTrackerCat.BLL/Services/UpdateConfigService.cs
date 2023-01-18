using Microsoft.Extensions.Logging;
using TaskTrackerCat.BLL.Services.Helpers;
using TaskTrackerCat.DAL.Models;
using TaskTrackerCat.DAL.Repositories.Interfaces;

namespace TaskTrackerCat.BLL.Services;

public class UpdateConfigService
{
    private readonly ConfigHelper _configHelper;
    private readonly IConfigRepository _configRepository;
    private readonly IDietRepository _dietRepository;

    private readonly ILogger<UpdateConfigService> _logger;

    private int countServingNumber;
    private DateTime estimatedDateFeeding;
    private TimeSpan INTERVAL;

    private int NUMBER_MEALS_PER_DAY;

    public UpdateConfigService(ConfigHelper configHelper, ILogger<UpdateConfigService> logger,
        IDietRepository dietRepository, IConfigRepository configRepository)
    {
        _configHelper = configHelper;

        _logger = logger;
        _dietRepository = dietRepository;
        _configRepository = configRepository;
    }

    public async void UpdateConfig(ConfigDto newConfig, ConfigDto pastConfig)
    {
        try
        {
            NUMBER_MEALS_PER_DAY = newConfig.NumberMealsPerDay;
            if (pastConfig.NumberMealsPerDay != newConfig.NumberMealsPerDay) await UpdateDiets(newConfig, pastConfig);

            await UpdateDateFeeding(newConfig);

            _logger.LogInformation("Пользователь изменил конфигурацию.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Ошибка изменения конфигурации.");
            await _configRepository.UpdateAsync(pastConfig);
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
    ///     Удаляет приемы еды, которые больше нового значения приемов еды в день.
    /// </summary>
    private async Task DeleteDiets()
    {
        var firstDayInCurrentMonth = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1);

        await _dietRepository.DeleteDietsAsync(NUMBER_MEALS_PER_DAY, firstDayInCurrentMonth);
    }

    /// <summary>
    ///     Добавляет новые приемы пищи в текущий и будущий месяц.
    ///     Стоит отметить что будущий месяц всего один - следущий от текущего, поэтому запрос на получение максимального
    ///     месяца не пишется.
    /// </summary>
    /// <param name="config">Конфигурация приемов еды.</param>
    /// <param name="pastNumberMealsPerDay">Количество примемов еды в день до изменения.</param>
    private async Task AddDiets(ConfigDto config, int pastNumberMealsPerDay)
    {
        //Редактирование даты приема еды начинается с текущего месяца.
        //Устанавливаем максимальное значение, так как при изменении даты кормления идет сортировка по дате.
        //Если дату не устанавить в максимальное значение то добавленые примемы будут первыми.
        estimatedDateFeeding = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1,
            config.EndFeeding.Hours,
            config.EndFeeding.Minutes,
            config.EndFeeding.Milliseconds);
        //Число порции начинается с последней прошлой.
        countServingNumber = pastNumberMealsPerDay;
        var numberDiets = GetTotalNumberDiets(pastNumberMealsPerDay);

        var diets = new List<DietDto>();
        for (var i = 0; i < numberDiets; i++) AddDiet(diets, pastNumberMealsPerDay);

        await _dietRepository.AddAsync(diets);
    }

    /// <summary>
    ///     Высчитывает количество приемов еды с первого текущего месяца по следующий.
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
    ///     Добавляет один прием еды в список.
    /// </summary>
    /// <param name="diets">список приемов еды.</param>
    /// <param name="pastNumberMealsPerDay">Значение количества примемов еды в день до изменения.</param>
    private void AddDiet(List<DietDto> diets, int pastNumberMealsPerDay)
    {
        countServingNumber++;
        var diet = new DietDto
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
        INTERVAL = _configHelper.GetIntervalFeeding(config);

        //Редактирование даты приема еды начинается с текущего месяца.
        var dateFeeding = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var diets = await _dietRepository.GetAsync(dateFeeding);
        AddDateFeeding(config, diets);

        await _dietRepository.UpdateAsync(diets);
    }

    private void AddDateFeeding(ConfigDto newConfig, IEnumerable<DietDto> diets)
    {
        estimatedDateFeeding = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
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
                estimatedDateFeeding = _configHelper.GetEndTimeFeeding(estimatedDateFeeding, newConfig);
                diet.EstimatedDateFeeding = estimatedDateFeeding;

                estimatedDateFeeding = _configHelper.GetStartTimeFeeding(estimatedDateFeeding, newConfig);
                estimatedDateFeeding = estimatedDateFeeding.AddDays(1); //Кормить каждый день.
                countServingNumber = 1;
                continue;
            }

            diet.EstimatedDateFeeding = estimatedDateFeeding;
            estimatedDateFeeding = estimatedDateFeeding.AddHours(INTERVAL.Hours).AddMinutes(INTERVAL.Minutes);
            countServingNumber++;
        }
    }
}