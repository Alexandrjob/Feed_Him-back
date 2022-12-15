using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Infrastructure.Handlers.Interfaces;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Controllers;

[AllowAnonymous]
[ApiController]
[Route("/api/users")]
public class UserController : ControllerBase
{
    private readonly IRequestAuthenticationHandler<AuthenticationUserViewModel, AuthenticationUserViewModel>
        _authenticationUserHandler;

    private readonly IRequestAuthorizeHandler<AuthorizeUserViewModel, AuthorizeUserViewModel> _authorizeUserHandler;
    private readonly IUserRepository _userRepository;
    private readonly IConfigRepository _configRepository;
    private readonly IGroupRepository _groupRepository;

    public UserController(
        IRequestAuthenticationHandler<AuthenticationUserViewModel, AuthenticationUserViewModel>
            authenticationUserHandler,
        IRequestAuthorizeHandler<AuthorizeUserViewModel, AuthorizeUserViewModel> authorizeUserHandler,
        IUserRepository userRepository, IConfigRepository configRepository, IGroupRepository groupRepository)
    {
        _authenticationUserHandler = authenticationUserHandler;
        _authorizeUserHandler = authorizeUserHandler;
        _userRepository = userRepository;
        _configRepository = configRepository;
        _groupRepository = groupRepository;
    }

    /// <summary>
    /// Gets information about the user, diet and users in the group.
    /// </summary>
    /// <returns></returns>
    /// <response code="200"></response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInfo()
    {
        //Получить имя и почту
        //Получить данные конфигурации если это создатель группы
        //Получить данные группы(Имя учатников, и какой-то идентификатор(почта, id) если это создатель группы)
        var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var tokenUserEmail = jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value;

        var currentUser = new UserDto
        {
            Email = tokenUserEmail
        };

        var user = await _userRepository.GetUserAsync(currentUser);
        var group = await _groupRepository.GetGroupAsync(user);
        var users = await _userRepository.GetUsersGroupAsync(group);

        var usersGroup = new List<UserViewModel>();
        foreach (var userDto in users)
        {
            if (userDto.Email != currentUser.Email)
            {
                var userViewModel = new UserViewModel()
                {
                    Name = userDto.Name,
                    Email = userDto.Email
                };
                usersGroup.Add(userViewModel);
            }
        }

        if (user.CurrentGroupId != user.NativeGroupId)
        {
            var shortResponse = new GetInfoViewModel
            {
                User = new UserViewModel
                {
                    Name = user.Name,
                    Email = user.Email
                },
                UsersGroup = usersGroup,
                IsCreator = false
            };

            return Ok(shortResponse);
        }

        var responseConfig = await _configRepository.GetConfigFromGroupAsync(group);
        var response = new GetInfoViewModel
        {
            User = new UserViewModel
            {
                Name = user.Name,
                Email = user.Email
            },
            UsersGroup = usersGroup,
            Config = new ConfigViewModel
            {
                NumberMealsPerDay = responseConfig.NumberMealsPerDay,
                StartFeeding = new DateTime().Add(responseConfig.StartFeeding),
                EndFeeding = new DateTime().Add(responseConfig.EndFeeding)
            },
            IsCreator = true
        };
        return Ok(response);
    }

    /// <summary>
    /// Creates a user and generates meals.
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Status code 200.</returns>
    /// <response code="200">If the user and meals is created.</response>
    /// <response code="409">If user with this email already exists.</response>
    /// <response code="500">If an error occurred during data generation.</response>
    [HttpPost("/api/users/Authorize")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorViewModel<AuthorizeUserViewModel>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Authorize(AuthorizeUserViewModel model)
    {
        var response = await _authorizeUserHandler.Handle(model);
        if (response == null)
        {
            var error = new ErrorViewModel<AuthorizeUserViewModel>()
            {
                Detail = "User with this email already exists.",
                ViewModel = model
            };
            return Conflict(error);
        }

        return Ok();
    }

    /// <summary>
    /// Get authentication token.
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Response json.</returns>
    /// <response code="200">If authentication successful.</response>
    /// <response code="400">If the user entered incorrect data.</response>
    [HttpPost("/api/users/Authentication")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationUserViewModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorViewModel<AuthenticationUserViewModel>))]
    public async Task<IResult> Authentication([FromBody] AuthenticationUserViewModel model)
    {
        var response = await _authenticationUserHandler.Handle(model);
        if (response == null)
        {
            var error = new ErrorViewModel<AuthenticationUserViewModel>()
            {
                Detail = "Incorrectly entered data.",
                ViewModel = model
            };

            return Results.BadRequest(error);
        }

        return Results.Json(response, statusCode: StatusCodes.Status200OK);
    }
}