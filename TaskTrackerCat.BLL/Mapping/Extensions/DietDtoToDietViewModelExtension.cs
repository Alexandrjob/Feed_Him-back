using AutoMapper;
using TaskTrackerCat.BLL.Mediator.ResponseCommands.Models;
using TaskTrackerCat.DAL.Models;

namespace TaskTrackerCat.BLL.Mapping.Extensions;

public static class DietDtoToDietViewModelExtension
{
    public static List<ResponseDietViewModel> MapList(this IMapper mapper, List<DietDto> diets)
    {
        // var result = new List<ResponseDietViewModel>();
        // foreach (var diet in diets)
        // {
        //     result.Add(mapper.Map<ResponseDietViewModel>(diet));
        // }   
        //
        // return result;
        //
        return diets.Select(mapper.Map<ResponseDietViewModel>).ToList();
    }
}