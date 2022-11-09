namespace TaskTrackerCat.HttpModels;

/// <summary>
/// Класс приемов еды.
/// </summary>
public class DietViewModel
{
    public int Id { get; set; }
    
    /// <summary>
    /// Имя кормящего.
    /// </summary>
    public string? WaiterName { get; set; }

    /// <summary>
    /// Дата кормления.
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// Статус выполнения задачи, покормлен/не покормлен.
    /// </summary>
    public bool Status { get; set; }
}