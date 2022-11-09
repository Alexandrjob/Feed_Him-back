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
    private readonly IDbConnectionFactory<SqlConnection> _dbConnectionFactory;
    private readonly IDietRepository _dietRepository;
    private readonly IConfigRepository _configRepository;
    
    private int NUMBER_MEALS_PER_DAY;
    private int countServingNumber;
    private DateTime estimatedDateFeeding;

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
            NumberMealsPerDay = model.NumberMealsPerDay
        };

        //Получение конфига перед обновлением.  
        ConfigDto pastConfig = await _configRepository.GetConfigAsync(newConfig);

        if (pastConfig.NumberMealsPerDay == newConfig.NumberMealsPerDay)
        {
            return;
        }

        await _configRepository.UpdateConfigAsync(newConfig);
        NUMBER_MEALS_PER_DAY = newConfig.NumberMealsPerDay;

        try
        {
            if (newConfig.NumberMealsPerDay < pastConfig.NumberMealsPerDay)
            {
                await DeleteDiets();
                return;
            }

            await AddDiets(pastConfig.NumberMealsPerDay);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await _configRepository.UpdateConfigAsync(pastConfig);
            throw;
        }
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
    /// <param name="pastNumberMealsPerDay">Количество примемов еды в день до изменения.</param>
    private async Task AddDiets(int pastNumberMealsPerDay)
    {
        //Редактирование даты приема еды начинается с текущего месяца.
        estimatedDateFeeding = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
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
}