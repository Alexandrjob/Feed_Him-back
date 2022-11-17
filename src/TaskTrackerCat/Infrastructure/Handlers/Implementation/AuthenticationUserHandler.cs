using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Infrastructure.Handlers.Interfaces;
using TaskTrackerCat.Infrastructure.Identity;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Infrastructure.Handlers.Implementation;

public class
    AuthenticationUserHandler : IRequestAuthenticationHandler<AuthenticationUserViewModel, AuthenticationUserViewModel>
{
    private readonly JwtTokenHelper _jwtTokenHelper;
    private readonly IUserRepository _userRepository;

    public AuthenticationUserHandler(JwtTokenHelper jwtTokenHelper, IUserRepository userRepository)
    {
        _jwtTokenHelper = jwtTokenHelper;
        _userRepository = userRepository;
    }

    public async Task<AuthenticationUserViewModel?> Handle(AuthenticationUserViewModel model)
    {
        var userDto = new UserDto()
        {
            Email = model.Email,
            Password = model.Password
        };
        var user = await _userRepository.GetUserAsync(userDto);

        if (!IsValid(user, model))
        {
            return null;
        }

        var token = _jwtTokenHelper.GetToken(user);

        var newModel = new AuthenticationUserViewModel()
        {
            Email = user.Email,
            Name = user.Name,
            Token = token.AccessToken
        };

        return newModel;
    }

    private bool IsValid(UserDto user, AuthenticationUserViewModel model)
    {
        if (user == null)
        {
            return false;
        }

        if (user.Password != model.Password)
        {
            return false;
        }

        return true;
    }
}