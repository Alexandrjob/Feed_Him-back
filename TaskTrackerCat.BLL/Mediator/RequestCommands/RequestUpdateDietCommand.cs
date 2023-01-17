using MediatR;
using TaskTrackerCat.BLL.Mediator.ResponseCommands;

namespace TaskTrackerCat.BLL.Mediator.RequestCommands;

public class RequestUpdateDietCommand : IRequest<ResponseUpdateDietCommand>
{
    public int Id { get; set; }

    /// <summary>
    ///     Имя кормящего.
    /// </summary>
    public string? WaiterName { get; set; }

    /// <summary>
    ///     Дата кормления.
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    ///     Статус выполнения задачи, покормлен/не покормлен.
    /// </summary>
    public bool Status { get; set; }

    public int RowArray { get; set; }
    public int ColumnArray { get; set; }
}