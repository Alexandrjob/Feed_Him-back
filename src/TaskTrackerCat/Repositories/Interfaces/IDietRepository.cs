using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Repositories.Interfaces;

public interface IDietRepository
{
    /// <summary>
    /// Получение данных о кормленнии в течении месяца.
    /// </summary>
    /// <returns></returns>
    public Task<List<DietDto>> GetDietsAsync();

    public Task UpdateAsync(DietDto diet);
}