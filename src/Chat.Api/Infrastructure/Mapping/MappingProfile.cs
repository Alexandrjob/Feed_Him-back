using AutoMapper;
using Chat.Api.HttpModels;
using Chat.Api.Infrastructure.Commands;
using Chat.Api.Repositories.Models;

namespace Chat.Api.Infrastructure.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        //HttpModels => Commands
        CreateMap<GetItemViewModel, GetItemCommand>().ReverseMap();
        CreateMap<ItemViewModel, PostItemCommand>().ReverseMap();

        //Commands => Dto
        CreateMap<GetItemCommand, ItemDto>().ReverseMap();
    }
}