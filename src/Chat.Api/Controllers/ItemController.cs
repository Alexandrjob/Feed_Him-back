using AutoMapper;
using Chat.Api.HttpModels;
using Chat.Api.Infrastructure.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Api.Controllers;

[ApiController]
[Route("/api/items")]
public class ItemController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public ItemController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetItemViewModel>> Get(int id)
    {
        var result = await _mediator.Send(_mapper.Map<GetItemCommand>(new GetItemViewModel() {Id = id}));
        var resultViewModel = _mapper.Map<GetItemViewModel>(result);

        return resultViewModel;
    }

    [HttpPost]
    public async Task<ActionResult<ItemViewModel>> Post(ItemViewModel model)
    {
        var result = await _mediator.Send(_mapper.Map<PostItemCommand>(model));
        var resultViewModel = _mapper.Map<ItemViewModel>(result);

        return resultViewModel;
    }
}
