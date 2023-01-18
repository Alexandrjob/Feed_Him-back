using AutoMapper;
using TaskTrackerCat.BLL.Mediator.RequestCommands;
using TaskTrackerCat.BLL.Mediator.ResponseCommands.Models;
using TaskTrackerCat.BLL.SignalR.Models;
using TaskTrackerCat.DAL.Models;

namespace TaskTrackerCat.BLL.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        //Models => Dto
        CreateMap<ResponseDietViewModel, DietDto>().ReverseMap();
        CreateMap<DietHubViewModel, DietDto>().ReverseMap();

        //Commands => Commands

        //Commands => Dto
        CreateMap<RequestUpdateDietCommand, DietDto>().ReverseMap();
    }
}