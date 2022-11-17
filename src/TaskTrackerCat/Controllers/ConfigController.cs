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
    /// <response code="400">If access is not allowed.</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorViewModel<ConfigViewModel>))]
    public async Task<IActionResult> Update(ConfigViewModel model)
    {
        var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var tokenGroupId = jwtToken.Claims.First(c => c.Type == ClaimTypes.GroupSid).Value;
        var user = new UserDto()
        {
            GroupId = Convert.ToInt32(tokenGroupId)
        };

        var group = await _groupRepository.GetGroupAsync(user);
        if (model.Id != group.ConfigId)
        {
            var error = new ErrorViewModel<ConfigViewModel>()
            {
                Detail = "Access error.",
                ViewModel = model
            };
            return BadRequest(error);
        }

        var request = new UpdateConfigCommand()
        {
            Model = model,
            Group = group
        };
        await _updateConfigHandler.Handle(request);
        return Ok();
    }
}