using TaskTrackerCat.Controllers;
using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Infrastructure.Handlers.Interfaces;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Infrastructure.Handlers;

public class AuthenticationUserHandler : IRequestAuthenticationHandler<AuthenticationUserViewModel, AuthenticationUserViewModel>
{
    private readonly JwtTokenHelper _jwtTokenHelper;
    private readonly IUserRepository _userRepository;
    
    public AuthenticationUserHandler(JwtTokenHelper jwtTokenHelper, IUserRepository userRepository)
    {
        _jwtTokenHelper = jwtTokenHelper;
        _userRepository = userRepository;
    }

    public async Task<AuthenticationUserViewModel> Handle(AuthenticationUserViewModel model)
    {
        var user = await _userRepository.GetUserAsync(model.Id);

        if (IsNotValid(user, model))
        {
            return new AuthenticationUserViewModel();
        }
        
        var token = _jwtTokenHelper.GetToken();

        var newModel = new AuthenticationUserViewModel()
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Token = token
        };
        
        return newModel;
    }
    
    private bool IsNotValid(UserDto user,AuthenticationUserViewModel model)
    {
        if (user == null)
        {
            return false;
        }

        if (user.Name != model.Name || 
            user.Password != model.Password)
        {
            return false;
        }

        return true;
    }
}