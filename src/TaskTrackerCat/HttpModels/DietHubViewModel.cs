using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.HttpModels;

public class DietHubViewModel : DietDto
{
    public int RowArray { get; set; }
    public int ColumnArray { get; set; }
}