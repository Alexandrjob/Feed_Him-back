using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TaskTrackerCat.BLL.Mapping.Extensions;
using TaskTrackerCat.BLL.Mediator.RequestCommands;
using TaskTrackerCat.BLL.Mediator.ResponseCommands;
using TaskTrackerCat.BLL.Mediator.ResponseCommands.Models;
using TaskTrackerCat.BLL.SignalR;
using TaskTrackerCat.DAL.Models;
using TaskTrackerCat.DAL.Repositories.Interfaces;

namespace TaskTrackerCat.BLL.Mediator.Handlers;

public class UpdateDietHandler : IRequestHandler<RequestUpdateDietCommand, ResponseUpdateDietCommand>
{
    private readonly IMemoryCache _cache;
    private readonly DietHub _dietHub;

    private readonly IDietRepository _dietRepository;
    private readonly ILogger<UpdateDietHandler> _logger;
    private readonly IMapper _mapper;

    public UpdateDietHandler(IMemoryCache cache, ILogger<UpdateDietHandler> logger, IMapper mapper,
        DietHub dietHub, IDietRepository dietRepository)
    {
        _cache = cache;
        _logger = logger;
        _mapper = mapper;
        _dietHub = dietHub;

        _dietRepository = dietRepository;
    }

    public async Task<ResponseUpdateDietCommand> Handle(RequestUpdateDietCommand request,
        CancellationToken cancellationToken)
    {
        var dietDto = _mapper.Map<DietDto>(request);

        var diet = await _dietRepository.UpdateAsync(dietDto);
        _logger.LogInformation("Пользователь {User} обновил прием пищи. Дата изменения: {Date}, статус: {Status}",
            request.WaiterName,
            request.Date,
            request.Status);

        _cache.TryGetValue("diets", out List<ResponseDietViewModel>? diets);
        if (diets != null)
        {
            var dietCashType = _mapper.Map<ResponseDietViewModel>(diet);
            var index = diets.IndexOf(dietCashType);
            diets[index] = dietCashType;
        }

        var updateDiet = _mapper.MapCombine(diet, request);
        await _dietHub.UpdateDietAsync(updateDiet);

        return new ResponseUpdateDietCommand();
    }
}