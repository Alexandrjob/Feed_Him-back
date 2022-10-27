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

    [HttpGet]
    public async Task<IResult> Get()
    {
        var result = await _dietRepository.GetDietsAsync();

        return Results.Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody]DietViewModel model)
    {
        var dietDto = new DietDto()
        {
            Id = model.Id,
            ServingNumber = model.ServingNumber,
            WaiterName = model.WaiterName,
            Date = model.Date,
            Status = model.Status
        };
        await _dietRepository.UpdateAsync(dietDto);

        return Ok();
    }
}