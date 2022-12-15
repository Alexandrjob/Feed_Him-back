using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Infrastructure.Handlers.Commands;
using TaskTrackerCat.Infrastructure.Handlers.Interfaces;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Controllers;

[Authorize]
[ApiController]
[Route("/api/configs")]
public class ConfigController : ControllerBase
{
    private readonly IRequestHandler<UpdateConfigCommand> _updateConfigHandler;

    private readonly IUserRepository _userRepository;
    private readonly IGroupRepository _groupRepository;

    public ConfigController(IRequestHandler<UpdateConfigCommand> updateConfigHandler, IGroupRepository groupRepository,
        IUserRepository userRepository)
    {
        _updateConfigHandler = updateConfigHandler;
        _groupRepository = groupRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Update config number of meals per day.
    /// </summary>
    /// <param name="model">Class view model.</param>
    /// <returns></returns>
    /// <response code="200">If access is allowed.</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(ConfigViewModel model)
    {
        var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var tokenUserEmail = jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value;
        var user = new UserDto()
        {
            Email = tokenUserEmail
        };

        //TODO: сделать проверку, что пользователь имеет право на изменение конфига(является создателем).
        user = await _userRepository.GetUserAsync(user);
        var group = await _groupRepository.GetGroupAsync(user);

        var request = new UpdateConfigCommand()
        {
            Model = model,
            Group = group
        };
        await _updateConfigHandler.Handle(request);
        return Ok();
    }
}