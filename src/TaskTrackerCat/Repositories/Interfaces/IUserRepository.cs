using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Repositories.Interfaces;

public interface IUserRepository
{
    public Task<UserDto> AddUserAsync(UserDto user);
    public Task<UserDto> GetUserAsync(UserDto user);
    public Task<List<UserDto>> GetUsersGroupAsync(GroupDto group);
    public Task UpdateEmailNameAsync(UserDto user);
    public Task UpdatePasswordAsync(UserDto user);
    public Task DeleteUserAsync(UserDto user);
}