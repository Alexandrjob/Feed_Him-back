using Chat.Api.Infrastructure.Commands;
using Chat.Api.Repositories.Interfaces;
using MediatR;

namespace Chat.Api.Infrastructure.Handlers;

public class GetItemHandler : IRequestHandler<GetItemCommand, GetItemCommand>
{
    private readonly IItemRepository _itemRepository;

    public GetItemHandler(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public async Task<GetItemCommand> Handle(GetItemCommand request, CancellationToken cancellationToken)
    {
        var resp = await _itemRepository.GetItemAsync(request.Id);

        if (resp == null)
        {
            throw new NullReferenceException($"Item with specified id not found");
        }

        var response = new GetItemCommand()
        {
            Id = resp.Id,
            Name = resp.Name,
            Status = (Status) resp.Status
        };

        return response;
    }
}