using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Infrastructure.Handlers.Interfaces;
using TaskTrackerCat.Infrastructure.InitServices;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Infrastructure.Handlers.Implementation;

public class AuthorizeUserHandler : IRequestAuthorizeHandler<AuthorizeUserViewModel, AuthorizeUserViewModel>
{
    private readonly IUserRepository _userRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IConfigRepository _configRepository;
    private readonly IDietRepository _dietRepository;
    private readonly InitDiets _initDiets;

    public AuthorizeUserHandler(IUserRepository userRepository, IGroupRepository groupRepository,
        IConfigRepository configRepository, InitDiets initDiets, IDietRepository dietRepository)
    {
        _userRepository = userRepository;
        _groupRepository = groupRepository;
        _configRepository = configRepository;
        _initDiets = initDiets;
        _dietRepository = dietRepository;
    }

    public async Task<AuthorizeUserViewModel?> Handle(AuthorizeUserViewModel model)
    {
        var userDto = new UserDto
        {
            Email = model.Email,
            Name = model.Name,
            Password = model.Password
        };

        var userCheck = await _userRepository.GetUserAsync(userDto);

        if (userCheck != null)
        {
            return null;
        }

        try
        {
            var newConfig = new ConfigDto()
            {
                NumberMealsPerDay = 3,
                StartFeeding = new TimeSpan(7, 30, 0),
                EndFeeding = new TimeSpan(23, 00, 0),
            };

            var config = await _configRepository.AddConfigAsync(newConfig);
            var group = await _groupRepository.AddGroupAsync(config);

            userDto.CurrentGroupId = group.Id;
            userDto.NativeGroupId = group.Id;
            var user = await _userRepository.AddUserAsync(userDto);
            await _initDiets.Init(group);
        }
        catch (Exception e)
        {
            var user = await _userRepository.GetUserAsync(userDto);
            var group = await _groupRepository.GetGroupAsync(user);
            var config = await _configRepository.GetConfigFromGroupAsync(group);

            await _userRepository.DeleteUserAsync(user);
            await _groupRepository.DeleteGroupAsync(group);
            await _configRepository.DeleteConfigAsync(config);
            await _dietRepository.DeleteDietsAsync(group);
            Console.WriteLine(e);
            throw;
        }

        return model;
    }
}