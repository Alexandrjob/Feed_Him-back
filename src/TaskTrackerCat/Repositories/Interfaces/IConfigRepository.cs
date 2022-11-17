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
    /// <param name="config">Класс конфигурации.</param>
    /// <returns>Класс конфигурации.</returns>
    Task<ConfigDto> GetConfigAsync(ConfigDto config);

    /// <summary>
    /// Получение конфигурации из группы.
    /// </summary>
    /// <param name="group">Класс конфигурации.</param>
    /// <returns></returns>
    Task<ConfigDto> GetConfigFromGroupAsync(GroupDto group);

    Task<ConfigDto> AddConfigAsync(ConfigDto config);
    Task DeleteConfigAsync(ConfigDto config);
}