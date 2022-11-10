using Microsoft.AspNetCore.Mvc;
using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Controllers;

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
    [HttpGet]
    public async Task<IResult> Get(GetDietsViewModel model)
    {
        //TODO: Написать валидность данных.
        var result = await _dietRepository.GetDietsAsync(model.GroupId);
        return Results.Json(result);
    }

    /// <summary>
    /// Update diet.
    /// </summary>
    /// <param name="model">Class view model.</param>
    /// <returns></returns>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody]DietViewModel model)
    {
        var dietDto = new DietDto()
        {
            Id = model.Id,
            WaiterName = model.WaiterName,
            Date = model.Date,
            Status = model.Status
        };
        
        await _dietRepository.UpdateDietAsync(dietDto);
        return Ok();
    }
}