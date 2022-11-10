using Microsoft.AspNetCore.Mvc;
using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Infrastructure.Handlers.Interfaces;
using TaskTrackerCat.Repositories.Interfaces;

namespace TaskTrackerCat.Controllers;

[ApiController]
[Route("/api/users")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IRequestAuthenticationHandler<AuthenticationUserViewModel, TokenViewModel> _authenticationUserHandler;
    private readonly IRequestHandler<AuthorizeUserViewModel> _authorizeUserHandler;
    
    public UserController(IUserRepository userRepository,
        IRequestAuthenticationHandler<AuthenticationUserViewModel, TokenViewModel> authenticationUserHandler,
        IRequestHandler<AuthorizeUserViewModel> authorizeUserHandler)
    {
        _userRepository = userRepository;
        _authenticationUserHandler = authenticationUserHandler;
        _authorizeUserHandler = authorizeUserHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Authorize(AuthorizeUserViewModel model)
    {
        await _authorizeUserHandler.Handle(model);
        return Ok();
    }
    
    [HttpPut]
    public async Task<IResult?> Authentication(AuthenticationUserViewModel model)
    {
        //Если авторизация не прошла, как это написать?
        var response = await _authenticationUserHandler.Handle(model);
        if (response.Name == null)
        {
            return null;
        }
        
        return Results.Json(response);
    }
}