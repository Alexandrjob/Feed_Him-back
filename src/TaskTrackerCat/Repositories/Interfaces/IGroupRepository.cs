using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Repositories.Interfaces;

public interface IGroupRepository
{
    public Task<GroupDto> AddGroupAsync(ConfigDto config);
}