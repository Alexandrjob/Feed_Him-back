using Chat.Api.Infrastructure.Commands;
using MediatR;

namespace Chat.Api.Infrastructure.Handlers;

public class PostItemHandler : IRequestHandler<PostItemCommand, PostItemCommand>
{
    public async Task<PostItemCommand> Handle(PostItemCommand request, CancellationToken cancellationToken)
    {
        var response = new PostItemCommand()
        {
            Id = request.Id + 1,
            Name = request.Name + " I handle PostItemHandler"
        };

        return response;
    }
}