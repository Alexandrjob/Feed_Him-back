using TaskTrackerCat.DAL.Models;

namespace TaskTrackerCat.BLL.SignalR.Models;

public class DietHubViewModel : DietDto
{
    public int RowArray { get; set; }
    public int ColumnArray { get; set; }
}