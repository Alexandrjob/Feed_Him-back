namespace TaskTrackerCat.DAL.Models;

/// <summary>
///     Класс конфигурации, отвечающий за приемы еды.
/// </summary>
public class ConfigDto
{
    public int Id { get; set; }

    /// <summary>
    ///     Количество приемов еды.
    /// </summary>
    public int NumberMealsPerDay { get; set; }

    /// <summary>
    ///     Начало кормления.
    /// </summary>
    public TimeSpan StartFeeding { get; set; }

    /// <summary>
    ///     Конец кормления.
    /// </summary>
    public TimeSpan EndFeeding { get; set; }
}