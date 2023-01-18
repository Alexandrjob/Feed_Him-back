using MediatR;
using TaskTrackerCat.BLL.Mediator.ResponseCommands;

namespace TaskTrackerCat.BLL.Mediator.RequestCommands;

public class RequestGetDietsCommand : IRequest<ResponseGetDietsCommand>
{
}