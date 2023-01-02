namespace TaskTrackerCat.Repositories.Models;

/// <summary>
/// Класс приемов еды.
/// </summary>
public class DietDto
{
    public int Id { get; set; }

    /// <summary>
    /// Номер порции. Их в одном дне должно быть не больше 3-х.
    /// </summary>
    public int ServingNumber { get; set; }

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

    /// <summary>
    /// Дата предпологаемого кормления.
    /// </summary>
    public DateTime EstimatedDateFeeding { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj == null || !(obj is DietDto)) return false;

        var other = (DietDto) obj;

        return Id == other.Id;
    }
}