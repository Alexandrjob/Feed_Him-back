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
    private NpgsqlConnection connection;
    private int daysInMonth;

    public InitService(IConfiguration configuration)
    {
        _Diets = new List<DietDto>();
        _connectionString = configuration.GetSection("ConnectionString").Value;
        estimatedDateFeeding = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
    }

    public async Task Init()
    {
        connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await InitMonth();
        _Diets.Clear();
        await InitNextMonth();
    }

    private async Task InitMonth()
    {
        //Приверка на существование данных в таблице.
        var sql = @"SELECT(SELECT count(*) FROM diets) = 0";
        var resultIsEmpty = await connection.QueryAsync<bool>(sql);
        if (!resultIsEmpty.FirstOrDefault())
        {
            return;
        }

        //Указываем что дата приема еды начинается с текущего месяца.
        estimatedDateFeeding = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        //Записываем количество дней в текущем месяце.
        daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
        await AddDiets();
    }

    private async Task InitNextMonth()
    {
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
        await AddDiets();
    }

    private async Task AddDiets()
    {
        var sql =
            "INSERT INTO diets " +
            "(serving_number, status, estimated_date_feeding) " +
            "VALUES(@ServingNumber, @Status, @EstimatedDateFeeding)";

        for (var i = 0; i < daysInMonth * numberMealsPerDay; i++)
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