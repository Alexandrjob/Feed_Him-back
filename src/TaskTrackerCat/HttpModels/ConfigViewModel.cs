namespace TaskTrackerCat.HttpModels;

/// <summary>
/// Класс конфигурации, отвечающий за количество приемов еды.
/// </summary>
public class ConfigViewModel
{
    public int Id { get; set; }
    
    /// <summary>
    /// Количество приемов еды.
    /// </summary>
    public int NumberMealsPerDay { get; set; }
}