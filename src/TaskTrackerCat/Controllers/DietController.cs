using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Controllers;

[Authorize]
[ApiController]
[Route("/api/diets")]
public class DietController : ControllerBase
{
    private readonly IDietRepository _dietRepository;

    public DietController(IDietRepository dietRepository)
    {
        _dietRepository = dietRepository;
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

        var tokenGroupId = jwtToken.Claims.First(c => c.Type == ClaimTypes.GroupSid).Value;

        var result = await _dietRepository.GetDietsAsync(Convert.ToInt32(tokenGroupId));
        return Results.Json(result);
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

        var tokenGroupId = jwtToken.Claims.First(c => c.Type == ClaimTypes.GroupSid).Value;

        var dietDto = new DietDto()
        {
            Id = model.Id,
            WaiterName = model.WaiterName,
            Date = model.Date,
            Status = model.Status
        };

        var diet = await _dietRepository.GetDietAsync(dietDto);
        if (diet.GroupId.ToString() != tokenGroupId)
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