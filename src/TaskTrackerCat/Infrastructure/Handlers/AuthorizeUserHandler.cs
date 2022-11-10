using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Infrastructure.Handlers.Interfaces;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Infrastructure.Handlers;

public class AuthorizeUserHandler:IRequestHandler<AuthorizeUserViewModel>
{
    private readonly IUserRepository _userRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IConfigRepository _configRepository;
    
    public AuthorizeUserHandler(IUserRepository userRepository, IGroupRepository groupRepository, IConfigRepository configRepository)
    {
        _userRepository = userRepository;
        _groupRepository = groupRepository;
        _configRepository = configRepository;
    }

    public async Task Handle(AuthorizeUserViewModel user)
    {
        var config = await _configRepository.AddConfigAsync();
        var group = await _groupRepository.AddGroupAsync(config);
        
        UserDto userDto = new UserDto
        {
            Email = user.Email,
            Name = user.Name,
            Password = user.Password,
            GroupId = group.Id,

        };
        await _userRepository.CreateUserAsync(userDto);
    }
}