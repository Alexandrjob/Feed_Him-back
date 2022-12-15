using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Infrastructure.Identity;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Controllers;

[AllowAnonymous]
[ApiController]
[Route("/api/groups")]
public class GroupController : ControllerBase
{
    private readonly JwtTokenHelper _jwtTokenHelper;

    private readonly IUserRepository _userRepository;
    private readonly IGroupRepository _groupRepository;

    public GroupController(
        JwtTokenHelper jwtTokenHelper,
        IUserRepository userRepository,
        IGroupRepository groupRepository)
    {
        _jwtTokenHelper = jwtTokenHelper;
        _userRepository = userRepository;
        _groupRepository = groupRepository;
    }

    /// <summary>
    /// Get link to join the group.
    /// </summary>
    /// <returns></returns>
    /// <response code="200">User is the creator of the group.</response>
    /// /// <response code="409">User is not the creator of the group.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public async Task<IActionResult> GetLink()
    {
        var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var tokenUserEmail = jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value;
        var tokenGroupId = jwtToken.Claims.First(c => c.Type == ClaimTypes.GroupSid).Value;

        var userDto = new UserDto
        {
            Email = tokenUserEmail
        };
        var group = new GroupDto
        {
            Id = Convert.ToInt32(tokenGroupId)
        };

        var user = await _userRepository.GetUserAsync(userDto);
        if (user.NativeGroupId != user.CurrentGroupId)
        {
            var error = new ErrorViewModel<AuthorizeUserViewModel>()
            {
                Detail = "User is not the creator of the group."
            };
            return Conflict(error);
        }

        var tokenGroupLink = _jwtTokenHelper.GetTokenGroup(group);

        // var host = HttpContext.Request.Host;
        // var url = HttpContext.Request.Path;
        var link = "http://localhost:3000/panel/group/invitation/" + user.Name + "/" + tokenGroupLink;

        return Ok(link);
    }

    /// <summary>
    /// Updates the user's group.
    /// </summary>
    /// <returns></returns>
    /// <response code="200">User group updated.</response>
    /// <response code="202">User is already a member of this group.</response>
    [HttpPost("/api/groups/update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Update(string tokenGroup)
    {
        var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        var jwtTokenLink = tokenHandler.ReadJwtToken(tokenGroup);

        var tokenUserEmail = jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value;
        var tokenGroupId = jwtTokenLink.Claims.First(c => c.Type == ClaimTypes.GroupSid).Value;

        var user = new UserDto
        {
            Email = tokenUserEmail,
        };
        var responseUser = await _userRepository.GetUserAsync(user);
        if (responseUser.CurrentGroupId == Convert.ToInt32(tokenGroupId))
        {
            return Accepted();
        }

        responseUser.CurrentGroupId = Convert.ToInt32(tokenGroupId);
        await _groupRepository.UpdateGroupAsync(responseUser);

        token = _jwtTokenHelper.GetToken(responseUser).AccessToken;
        return Ok(token);
    }

    /// <summary>
    /// Restore the user's group.
    /// </summary>
    /// <returns></returns>
    /// <response code="200">User group restored.</response>
    /// <response code="202">User group already restored.</response>
    [HttpPost("/api/groups/restore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Restore()
    {
        var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var tokenUserEmail = jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value;

        var user = new UserDto
        {
            Email = tokenUserEmail,
        };
        var responseUser = await _userRepository.GetUserAsync(user);
        if (responseUser.CurrentGroupId == responseUser.NativeGroupId)
        {
            return Accepted();
        }

        responseUser.CurrentGroupId = responseUser.NativeGroupId;
        await _groupRepository.UpdateGroupAsync(responseUser);

        token = _jwtTokenHelper.GetToken(responseUser).AccessToken;
        return Ok(token);
    }
}