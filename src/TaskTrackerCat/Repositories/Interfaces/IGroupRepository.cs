using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Repositories.Interfaces;

public interface IGroupRepository
{
    public Task<GroupDto> AddGroupAsync(ConfigDto config);
    public Task<GroupDto> GetGroupAsync(UserDto user);
    Task<List<GroupDto>> GetAllGroupsAsync();
    Task DeleteGroupAsync(GroupDto group);
}