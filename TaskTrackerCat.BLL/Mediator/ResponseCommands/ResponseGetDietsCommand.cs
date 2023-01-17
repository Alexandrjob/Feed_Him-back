using TaskTrackerCat.BLL.Mediator.ResponseCommands.Models;

namespace TaskTrackerCat.BLL.Mediator.ResponseCommands;

public class ResponseGetDietsCommand
{
    public ResponseGetDietsCommand(List<ResponseDietViewModel> diets)
    {
        Diets = diets;
    }

    public List<ResponseDietViewModel> Diets { get; set; }
}