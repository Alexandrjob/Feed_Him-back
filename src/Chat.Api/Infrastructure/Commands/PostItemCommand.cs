using MediatR;

namespace Chat.Api.Infrastructure.Commands;

public class PostItemCommand : IRequest<PostItemCommand>
{
    public int Id { get; set; }
    public string Name { get; set; }
}