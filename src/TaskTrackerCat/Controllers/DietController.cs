using Microsoft.AspNetCore.Mvc;
using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Infrastructure;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Controllers;

[ApiController]
[Route("/api/diets")]
public class DietController : ControllerBase
{
    private readonly DietHub _dietHub;
    private readonly IDietRepository _dietRepository;

    public DietController(DietHub dietHub, IDietRepository dietRepository)
    {
        _dietHub = dietHub;
        _dietRepository = dietRepository;
    }

    /// <summary>
    /// Get diets current month.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IResult> Get()
    {
        var result = await _dietRepository.GetDietsAsync();
        return Results.Json(result);
    }

    /// <summary>
    /// Update diet.
    /// </summary>
    /// <param name="model">Class view model.</param>
    /// <returns></returns>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] DietViewModel model)
    {
        var dietDto = new DietDto()
        {
            Id = model.Id,
            WaiterName = model.WaiterName,
            Date = model.Date,
            Status = model.Status
        };

        var diet = await _dietRepository.UpdateDietAsync(dietDto);
        var updateDiet = new DietHubViewModel()
        {
            Id = diet.Id,
            WaiterName = diet.WaiterName,
            Date = diet.Date,
            Status = diet.Status,
            ServingNumber = diet.ServingNumber,
            EstimatedDateFeeding = diet.EstimatedDateFeeding,
            RowArray = model.RowArray,
            ColumnArray = model.ColumnArray
        };
        await _dietHub.UpdateDietAsync(updateDiet);
        return Ok();
    }
}