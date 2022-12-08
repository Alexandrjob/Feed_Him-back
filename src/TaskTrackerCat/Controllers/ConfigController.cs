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
    private readonly IGroupRepository _groupRepository;

    public ConfigController(IRequestHandler<UpdateConfigCommand> updateConfigHandler, IGroupRepository groupRepository)
    {
        _updateConfigHandler = updateConfigHandler;
        _groupRepository = groupRepository;
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

        var tokenGroupId = jwtToken.Claims.First(c => c.Type == ClaimTypes.GroupSid).Value;
        var user = new UserDto()
        {
            CurrentGroupId = Convert.ToInt32(tokenGroupId)
        };

        //TODO: сделать проверку, что пользователь имеет право на изменение конфига(является создателем).

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