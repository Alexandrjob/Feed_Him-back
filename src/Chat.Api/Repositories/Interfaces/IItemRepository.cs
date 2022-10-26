using Chat.Api.Repositories.Models;

namespace Chat.Api.Repositories.Interfaces;

public interface IItemRepository
{
    public Task<ItemDto> GetItemAsync(int id);
}