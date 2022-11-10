using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Repositories.Interfaces;

public interface IUserRepository
{
    public Task<UserDto> CreateUserAsync(UserDto user);
    public Task<UserDto> GetUserAsync(int id);
    public Task UpdateUserAsync(UserDto user);
    public Task DeleteUserAsync(UserDto user);
}