using MediatR;

namespace Chat.Api.Infrastructure.Commands;

public class GetItemCommand : IRequest<GetItemCommand>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Status Status { get; set; }
}

public enum Status
{
    Start,
    Completed
}