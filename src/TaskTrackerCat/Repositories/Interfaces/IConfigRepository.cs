using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Repositories.Interfaces;

public interface IConfigRepository
{
    /// <summary>
    /// Обновление количества приемов еды.
    /// </summary>
    /// <param name="config">Класс dto.</param>
    /// <returns></returns>
    public Task UpdateConfigAsync(ConfigDto config);

    /// <summary>
    /// Получение конфигурации.
    /// </summary>
    /// <returns>Класс dto.</returns>
    Task<ConfigDto> GetConfigAsync(ConfigDto config);
    
    
    Task<ConfigDto> AddConfigAsync();
}