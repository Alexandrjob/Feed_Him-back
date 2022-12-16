using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Infrastructure.Handlers.Interfaces;
using TaskTrackerCat.Infrastructure.Identity;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Controllers;

[AllowAnonymous]
[ApiController]
[Route("/api/users")]
public class UserController : ControllerBaseCastom
{
    private readonly IRequestAuthenticationHandler<AuthenticationUserViewModel, AuthenticationUserViewModel>
        _authenticationUserHandler;

    private readonly IRequestAuthorizeHandler<AuthorizeUserViewModel, AuthorizeUserViewModel> _authorizeUserHandler;
    private readonly JwtTokenHelper _jwtTokenHelper;
    private readonly IConfigRepository _configRepository;
    private readonly IGroupRepository _groupRepository;

    public UserController(
        IRequestAuthenticationHandler<AuthenticationUserViewModel, AuthenticationUserViewModel>
            authenticationUserHandler,
        IRequestAuthorizeHandler<AuthorizeUserViewModel, AuthorizeUserViewModel> authorizeUserHandler,
        JwtTokenHelper jwtTokenHelper,
        IConfigRepository configRepository,
        IGroupRepository groupRepository,
        IUserRepository userRepository) : base(userRepository)
    {
        _authenticationUserHandler = authenticationUserHandler;
        _authorizeUserHandler = authorizeUserHandler;
        _configRepository = configRepository;
        _groupRepository = groupRepository;
        _jwtTokenHelper = jwtTokenHelper;
    }

    /// <summary>
    /// Gets information about the user, diet and users in the group.
    /// </summary>
    /// <returns></returns>
    /// <response code="200"></response>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInfo()
    {
        var user = await GetUserAsync();
        var group = await _groupRepository.GetGroupAsync(user);
        var users = await _userRepository.GetUsersGroupAsync(group);

        var usersGroup = new List<UserViewModel>();
        foreach (var userDto in users)
        {
            if (userDto.Email != user.Email)
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
    /// Update email user.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    /// <response code="200">If the data is valid.</response>
    /// <response code="400">If the data is not valid.</response>
    [Authorize]
    [HttpPost("/api/users/update/email")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorViewModel<UserViewModel>))]
    public async Task<IActionResult> UpdateEmail(UserViewModel model)
    {
        var currentUser = await GetUserAsync();
        var updateUser = new UserDto()
        {
            Id = currentUser.Id,
            Name = currentUser.Name,
            Email = currentUser.Email
        };

        if (model.Name != null)
        {
            updateUser.Name = model.Name;
        }

        if (model.Email != null)
        {
            var userEmail = new UserDto()
            {
                Email = model.Email
            };
            userEmail = await _userRepository.GetUserAsync(userEmail);

            if (!IsValidEmail(model, userEmail, out var error))
            {
                return BadRequest(error);
            }

            updateUser.Email = model.Email;
        }

        await _userRepository.UpdateEmailNameAsync(updateUser);
        var token = _jwtTokenHelper.GetToken(updateUser).AccessToken;
        return Ok(token);
    }

    /// <summary>
    /// Update password user.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    /// <response code="200">If the data is valid.</response>
    /// <response code="400">If the data is not valid.</response>
    [Authorize]
    [HttpPost("/api/users/update/password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorViewModel<UserViewModel>))]
    public async Task<IActionResult> UpdatePassword(UserViewModel model)
    {
        var currentUser = await GetUserAsync();
        if (!IsValidPassword(model, currentUser, out var error))
        {
            BadRequest(error);
        }

        var user = new UserDto()
        {
            Email = model.Email,
            Password = model.NewPassword
        };

        await _userRepository.UpdatePasswordAsync(user);
        return Ok();
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

    private bool IsValidEmail(UserViewModel model, UserDto user, out ErrorViewModel<UserViewModel>? error)
    {
        if (user != null)
        {
            error = new ErrorViewModel<UserViewModel>()
            {
                Detail = "User with this email already exists.",
                ViewModel = model
            };

            return false;
        }

        error = null;
        return true;
    }

    private bool IsValidPassword(UserViewModel model, UserDto currentUser, out ErrorViewModel<UserViewModel>? error)
    {
        if (model.CurrentPassword != null ||
            model.NewPassword != null ||
            model.ConfirmPassword != null)
        {
            error = new ErrorViewModel<UserViewModel>()
            {
                Detail = "One of the parameters does not matter.",
                ViewModel = model
            };
            return false;
        }

        if (currentUser.Password != model.CurrentPassword)
        {
            error = new ErrorViewModel<UserViewModel>()
            {
                Detail = "Invalid user password.",
                ViewModel = model
            };

            return false;
        }

        if (model.NewPassword != model.ConfirmPassword)
        {
            error = new ErrorViewModel<UserViewModel>()
            {
                Detail = "New password does not match.",
                ViewModel = model
            };

            return false;
        }

        error = null;
        return true;
    }
}