namespace TaskTrackerCat.HttpModels;

public class GetInfoViewModel
{
    public UserViewModel User { get; set; }
    public List<UserViewModel> UsersGroup { get; set; }
    public ConfigViewModel Config { get; set; }
    public bool IsCreator { get; set; }
}