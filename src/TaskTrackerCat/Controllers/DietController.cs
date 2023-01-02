using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Infrastructure;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Controllers;

[ApiController]
[Route("/api/diets")]
public class DietController : ControllerBase
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<DietController> _logger;
    private readonly DietHub _dietHub;
    private readonly IDietRepository _dietRepository;

    public DietController(IMemoryCache cache, ILogger<DietController> logger, DietHub dietHub,
        IDietRepository dietRepository)
    {
        _cache = cache;
        _logger = logger;
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
        _cache.TryGetValue("key", out List<DietDto>? diets);
        if (diets != null) return Results.Json(diets);

        var result = await _dietRepository.GetDietsAsync();
        _cache.Set("key", result, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(1)));
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
        _logger.LogInformation("Пользователь {User} обновил прием пищи. Дата изменения: {Date}, статус: {Status}",
            model.WaiterName,
            model.Date,
            model.Status);

        _cache.TryGetValue("key", out List<DietDto>? diets);
        if (diets != null)
        {
            var index = diets.IndexOf(diet);
            diets[index] = diet;
        }

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