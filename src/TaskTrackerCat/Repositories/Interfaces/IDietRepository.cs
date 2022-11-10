using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Repositories.Interfaces;

public interface IDietRepository
{
    /// <summary>
    /// Получение данных о кормленнии в течении месяца.
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns>Список приемов еды за текущий месяц.</returns>
    public Task<List<DietDto>> GetDietsAsync(int groupId);
    
    /// <summary>
    /// Обновление статуса приема еды.
    /// </summary>
    /// <param name="diet">Класс dto.</param>
    /// <returns></returns>
    public Task UpdateDietAsync(DietDto diet);
}