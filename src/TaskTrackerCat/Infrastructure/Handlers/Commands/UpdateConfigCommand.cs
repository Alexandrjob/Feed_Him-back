using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Infrastructure.Handlers.Commands;

public class UpdateConfigCommand
{
    public ConfigViewModel Model { get; set; }
    public GroupDto Group { get; set; }
}