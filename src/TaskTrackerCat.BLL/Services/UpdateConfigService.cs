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

    private ConfigDto NewConfig { get; set; }
    private ConfigDto PastConfig { get; set; }
    
    private int countServingNumber;
    private DateTime estimatedDateFeeding;
    private TimeSpan INTERVAL;

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
            NewConfig = newConfig;
            PastConfig = pastConfig;
            
            if (PastConfig.NumberMealsPerDay != NewConfig.NumberMealsPerDay) await UpdateDiets();

            await UpdateDateFeeding();

            _logger.LogInformation("Пользователь изменил конфигурацию.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Ошибка изменения конфигурации.");
            await _configRepository.UpdateAsync(PastConfig);
            throw;
        }
    }

    private async Task UpdateDiets()
    {
        if (NewConfig.NumberMealsPerDay < PastConfig.NumberMealsPerDay)
        {
            await DeleteDiets();
            return;
        }

        await AddDiets();
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

        await _dietRepository.DeleteDietsAsync(NewConfig.NumberMealsPerDay, firstDayInCurrentMonth);
    }

    /// <summary>
    ///     Добавляет новые приемы пищи в текущий и будущий месяц.
    ///     Стоит отметить что будущий месяц всего один - следущий от текущего, поэтому запрос на получение максимального
    ///     месяца не пишется.
    /// </summary>
    private async Task AddDiets()
    {
        //Редактирование даты приема еды начинается с текущего месяца.
        //Устанавливаем максимальное значение, так как при изменении даты кормления идет сортировка по дате.
        //Если дату не устанавить в максимальное значение то добавленые примемы будут первыми.
        estimatedDateFeeding = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1,
            NewConfig.EndFeeding.Hours,
            NewConfig.EndFeeding.Minutes,
            NewConfig.EndFeeding.Milliseconds);
        //Число порции начинается с последней прошлой.
        countServingNumber = PastConfig.NumberMealsPerDay;
        var numberDiets = GetTotalNumberDiets();

        var diets = new List<DietDto>();
        for (var i = 0; i < numberDiets; i++) AddDiet(diets);

        await _dietRepository.AddAsync(diets);
    }

    /// <summary>
    ///     Высчитывает количество приемов еды с первого текущего месяца по следующий.
    /// </summary>
    /// <returns>Количесво приемов еды.</returns>
    private int GetTotalNumberDiets()
    {
        var daysInCurrentMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
        var daysInNextMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.AddMonths(1).Month);

        return (daysInNextMonth + daysInCurrentMonth) * (NewConfig.NumberMealsPerDay - PastConfig.NumberMealsPerDay);
    }

    /// <summary>
    ///     Добавляет один прием еды в список.
    /// </summary>
    /// <param name="diets">список приемов еды.</param>
    private void AddDiet(List<DietDto> diets)
    {
        countServingNumber++;
        var diet = new DietDto
        {
            ServingNumber = countServingNumber,
            Status = false,
            EstimatedDateFeeding = estimatedDateFeeding
        };

        if (countServingNumber == NewConfig.NumberMealsPerDay)
        {
            countServingNumber = PastConfig.NumberMealsPerDay;
            estimatedDateFeeding = estimatedDateFeeding.AddDays(1); //Кормить каждый день.
        }

        diets.Add(diet);
    }

    private async Task UpdateDateFeeding()
    {
        INTERVAL = _configHelper.GetIntervalFeeding(NewConfig);

        //Редактирование даты приема еды начинается с текущего месяца.
        var dateFeeding = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var diets = await _dietRepository.GetAsync(dateFeeding);
        AddDateFeeding(diets);

        await _dietRepository.UpdateAsync(diets);
    }

    private void AddDateFeeding(IEnumerable<DietDto> diets)
    {
        estimatedDateFeeding = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1,
            NewConfig.StartFeeding.Hours,
            NewConfig.StartFeeding.Minutes,
            NewConfig.StartFeeding.Milliseconds);

        //Число порции начинается с первой.
        countServingNumber = 1;

        foreach (var diet in diets)
        {
            if (countServingNumber == NewConfig.NumberMealsPerDay)
            {
                estimatedDateFeeding = _configHelper.GetEndTimeFeeding(estimatedDateFeeding, NewConfig);
                diet.EstimatedDateFeeding = estimatedDateFeeding;

                estimatedDateFeeding = _configHelper.GetStartTimeFeeding(estimatedDateFeeding, NewConfig);
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