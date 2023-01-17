using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerCat.BLL.Mediator.RequestCommands;

namespace TaskTrackerCat.API.Controllers;

[ApiController]
[Route("/api/diets")]
public class DietController : ControllerBase
{
    private readonly IMediator _mediator;

    public DietController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    ///     Get diets current month.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IResult> Get()
    {
        var response = await _mediator.Send(new RequestGetDietsCommand());
        return Results.Json(response);
    }

    /// <summary>
    ///     Update diet.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] RequestUpdateDietCommand request)
    {
        await _mediator.Send(request);
        return Ok();
    }
}