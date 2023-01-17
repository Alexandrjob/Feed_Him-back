using TaskTrackerCat.DAL.Models;

namespace TaskTrackerCat.DAL.Repositories.Interfaces;

public interface IDietRepository
{
    /// <summary>
    ///     Получение данных о кормленнии в течении месяца.
    /// </summary>
    /// <returns>Список приемов еды за текущий месяц.</returns>
    public Task<List<DietDto>> GetAsync();

    /// <summary>
    ///     Получает все приемы пищи с текущего месяца.
    /// </summary>
    /// <param name="dateFeeding"></param>
    /// <returns></returns>
    public Task<List<DietDto>> GetAsync(DateTime dateFeeding);

    /// <summary>
    ///     Обновление статуса приема еды.
    /// </summary>
    /// <param name="diet">Класс dto.</param>
    /// <returns></returns>
    public Task<DietDto> UpdateAsync(DietDto diet);

    /// <summary>
    ///     Обновляет список приемов пищи.
    /// </summary>
    /// <param name="diets"></param>
    /// <returns></returns>
    public Task UpdateAsync(List<DietDto> diets);

    /// <summary>
    ///     Добавляет новые приемы пищи в текущий и будущий месяц.
    ///     Стоит отметить что будущий месяц всего один - следущий от текущего, поэтому запрос на получение максимального
    ///     месяца не пишется.
    /// </summary>
    /// <param name="diets"></param>
    /// <returns></returns>
    public Task AddAsync(List<DietDto> diets);

    /// <summary>
    ///     Удаляет приемы еды, которые больше значения приемов еды в день.
    ///     Начиная с указанного месяца.
    /// </summary>
    public Task DeleteDietsAsync(int numberMealsPerDay, DateTime firstDayInCurrentMonth);
}