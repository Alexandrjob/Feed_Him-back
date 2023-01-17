using TaskTrackerCat.DAL.Models;

namespace TaskTrackerCat.DAL.Repositories.Interfaces;

public interface IConfigRepository
{
    /// <summary>
    ///     Добавляет конфишурацию.
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public Task AddAsync(ConfigDto config);

    /// <summary>
    ///     Обновление количества приемов еды.
    /// </summary>
    /// <param name="config">Класс dto.</param>
    /// <returns></returns>
    public Task UpdateAsync(ConfigDto config);

    /// <summary>
    ///     Получение конфигурации.
    /// </summary>
    /// <returns>Класс dto.</returns>
    Task<ConfigDto> GetAsync(ConfigDto config);
}