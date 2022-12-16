namespace TaskTrackerCat.HttpModels;

public class UserViewModel
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}