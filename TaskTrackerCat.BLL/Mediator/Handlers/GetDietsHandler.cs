using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TaskTrackerCat.BLL.Mapping.Extensions;
using TaskTrackerCat.BLL.Mediator.RequestCommands;
using TaskTrackerCat.BLL.Mediator.ResponseCommands;
using TaskTrackerCat.BLL.Mediator.ResponseCommands.Models;
using TaskTrackerCat.DAL.Repositories.Interfaces;

namespace TaskTrackerCat.BLL.Mediator.Handlers;

public class GetDietsHandler : IRequestHandler<RequestGetDietsCommand, ResponseGetDietsCommand>
{
    private readonly IMemoryCache _cache;

    private readonly IDietRepository _dietRepository;
    private readonly ILogger<GetDietsHandler> _logger;
    private readonly IMapper _mapper;

    public GetDietsHandler(IMemoryCache cache, ILogger<GetDietsHandler> logger, IMapper mapper,
        IDietRepository dietRepository)
    {
        _cache = cache;
        _logger = logger;
        _mapper = mapper;

        _dietRepository = dietRepository;
    }

    public async Task<ResponseGetDietsCommand> Handle(RequestGetDietsCommand request,
        CancellationToken cancellationToken)
    {
        _cache.TryGetValue("diets", out List<ResponseDietViewModel>? diets);
        if (diets != null)
            return new ResponseGetDietsCommand(diets);

        var response = _mapper.MapList(await _dietRepository.GetAsync());

        _cache.Set("diets", response, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(1)));
        return new ResponseGetDietsCommand(response);
    }
}