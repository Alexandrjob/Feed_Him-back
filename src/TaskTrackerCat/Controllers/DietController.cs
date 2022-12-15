using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Infrastructure.Identity;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Controllers;

[Authorize]
[ApiController]
[Route("/api/diets")]
public class DietController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IDietRepository _dietRepository;

    public DietController(IDietRepository dietRepository,
        IUserRepository userRepository)
    {
        _dietRepository = dietRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Get diets current month.
    /// </summary>
    /// <returns></returns>
    /// /// <response code="200"></response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DietDto>))]
    public async Task<IResult> Get()
    {
        var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var tokenUserEmail = jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value;

        var user = new UserDto
        {
            Email = tokenUserEmail,
        };
        user = await _userRepository.GetUserAsync(user);

        var result = await _dietRepository.GetDietsAsync(user.CurrentGroupId);
        return Results.Json(result, statusCode: StatusCodes.Status200OK);
    }

    /// <summary>
    /// Update diet.
    /// </summary>
    /// <param name="model">Class view model.</param>
    /// <returns></returns>
    ///  <response code="200">If access is allowed.</response>
    /// <response code="400">If access is not allowed.</response>
    [HttpPut]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorViewModel<DietViewModel>))]
    public async Task<IActionResult> Update([FromBody] DietViewModel model)
    {
        var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var tokenUserEmail = jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value;

        var user = new UserDto
        {
            Email = tokenUserEmail,
        };
        user = await _userRepository.GetUserAsync(user);

        var dietDto = new DietDto()
        {
            Id = model.Id,
            WaiterName = model.WaiterName,
            Date = model.Date,
            Status = model.Status
        };

        var diet = await _dietRepository.GetDietAsync(dietDto);
        if (diet.GroupId != user.CurrentGroupId)
        {
            var error = new ErrorViewModel<DietViewModel>()
            {
                Detail = "Access error.",
                ViewModel = model
            };
            return BadRequest(error);
        }

        await _dietRepository.UpdateDietAsync(dietDto);
        return Ok();
    }
}