using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Infrastructure.Handlers.Interfaces;

namespace TaskTrackerCat.Controllers;

[AllowAnonymous]
[ApiController]
[Route("/api/users")]
public class UserController : ControllerBase
{
    private readonly IRequestAuthenticationHandler<AuthenticationUserViewModel, AuthenticationUserViewModel>
        _authenticationUserHandler;

    private readonly IRequestAuthorizeHandler<AuthorizeUserViewModel, AuthorizeUserViewModel> _authorizeUserHandler;

    public UserController(
        IRequestAuthenticationHandler<AuthenticationUserViewModel, AuthenticationUserViewModel>
            authenticationUserHandler,
        IRequestAuthorizeHandler<AuthorizeUserViewModel, AuthorizeUserViewModel> authorizeUserHandler)
    {
        _authenticationUserHandler = authenticationUserHandler;
        _authorizeUserHandler = authorizeUserHandler;
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