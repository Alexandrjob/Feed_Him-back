using Microsoft.AspNetCore.Mvc;
using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Infrastructure.Handlers.Interfaces;

namespace TaskTrackerCat.Controllers;

[ApiController]
[Route("/api/configs")]
public class ConfigController : ControllerBase
{
    private readonly IRequestHandler<ConfigViewModel> _updateConfigHandler;

    public ConfigController(IRequestHandler<ConfigViewModel> updateConfigHandler)
    {
        _updateConfigHandler = updateConfigHandler;
    }

    /// <summary>
    /// Update config number of meals per day.
    /// </summary>
    /// <param name="model">Class view model.</param>
    /// <returns></returns>
    [HttpPut]
    public async Task<IActionResult> Update(ConfigViewModel model)
    {
        await _updateConfigHandler.Handle(model);
        return Ok();
    }
}