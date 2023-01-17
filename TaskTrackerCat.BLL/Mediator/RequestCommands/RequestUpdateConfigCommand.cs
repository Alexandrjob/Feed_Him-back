using MediatR;
using TaskTrackerCat.BLL.Mediator.ResponseCommands;

namespace TaskTrackerCat.BLL.Mediator.RequestCommands;

public class RequestUpdateConfigCommand : IRequest<ResponseUpdateConfigCommand>
{
    public int Id { get; set; }

    /// <summary>
    ///     Количество приемов еды.
    /// </summary>
    public int NumberMealsPerDay { get; set; }

    /// <summary>
    ///     Начало кормления.
    /// </summary>
    public DateTime StartFeeding { get; set; }

    /// <summary>
    ///     Конец кормления.
    /// </summary>
    public DateTime EndFeeding { get; set; }
}