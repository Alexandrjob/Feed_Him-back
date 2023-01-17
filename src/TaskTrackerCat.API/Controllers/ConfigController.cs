using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerCat.BLL.Mediator.RequestCommands;

namespace TaskTrackerCat.API.Controllers;

[ApiController]
[Route("/api/configs")]
public class ConfigController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConfigController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    ///     Update config number of meals per day.
    /// </summary>
    /// <param name="request">Class view model.</param>
    /// <returns></returns>
    [HttpPut]
    public async Task<IActionResult> Update(RequestUpdateConfigCommand request)
    {
        await _mediator.Send(request);
        return Ok();
    }
}