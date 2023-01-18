using MediatR;
using TaskTrackerCat.BLL.Mediator.RequestCommands;
using TaskTrackerCat.BLL.Mediator.ResponseCommands;
using TaskTrackerCat.BLL.Services;
using TaskTrackerCat.DAL.Models;
using TaskTrackerCat.DAL.Repositories.Interfaces;

namespace TaskTrackerCat.BLL.Mediator.Handlers;

public class UpdateConfigHandler : IRequestHandler<RequestUpdateConfigCommand, ResponseUpdateConfigCommand>
{
    private readonly UpdateConfigService _updateConfigService;
    private readonly IConfigRepository _configRepository;

    public UpdateConfigHandler(
        IConfigRepository configRepository, UpdateConfigService updateConfigService)
    {
        _configRepository = configRepository;
        _updateConfigService = updateConfigService;
    }

    /// <summary>
    ///     Обновляет количество приемов еды в день.
    /// </summary>
    /// <param name="request"></param>
    public async Task<ResponseUpdateConfigCommand> Handle(RequestUpdateConfigCommand request,
        CancellationToken cancellationToken)
    {
        var newConfig = new ConfigDto
        {
            NumberMealsPerDay = request.NumberMealsPerDay,
            StartFeeding = new TimeSpan(request.StartFeeding.Hour, request.StartFeeding.Minute,
                request.StartFeeding.Second),
            EndFeeding = new TimeSpan(request.EndFeeding.Hour, request.EndFeeding.Minute, request.EndFeeding.Second)
        };

        //Получение конфига перед обновлением.  
        var pastConfig = await _configRepository.GetAsync(newConfig);

        if (pastConfig.NumberMealsPerDay == newConfig.NumberMealsPerDay &&
            pastConfig.StartFeeding == newConfig.StartFeeding &&
            pastConfig.EndFeeding == newConfig.EndFeeding)
            return new ResponseUpdateConfigCommand();
        
        newConfig.Id = pastConfig.Id;
        await _configRepository.UpdateAsync(newConfig);

        _updateConfigService.UpdateConfig(newConfig, pastConfig);
        return new ResponseUpdateConfigCommand();
    }
}