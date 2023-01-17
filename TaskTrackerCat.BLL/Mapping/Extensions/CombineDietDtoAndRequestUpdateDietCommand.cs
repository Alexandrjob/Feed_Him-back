using AutoMapper;
using TaskTrackerCat.BLL.Mediator.RequestCommands;
using TaskTrackerCat.BLL.SignalR.Models;
using TaskTrackerCat.DAL.Models;

namespace TaskTrackerCat.BLL.Mapping.Extensions;

public static class CombineDietDtoAndRequestUpdateDietCommand
{
    public static DietHubViewModel MapCombine(this IMapper mapper, DietDto dto, RequestUpdateDietCommand request)
    {
        var model = mapper.Map<DietHubViewModel>(dto);
        model.RowArray = request.RowArray;
        model.ColumnArray = request.ColumnArray;

        return model;
    }
}