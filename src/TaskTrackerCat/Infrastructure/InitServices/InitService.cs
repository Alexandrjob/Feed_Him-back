using TaskTrackerCat.Repositories.Interfaces;

namespace TaskTrackerCat.Infrastructure.InitServices;

public class InitService
{
    private readonly IServiceProvider _serviceProvider;
    private IGroupRepository _groupRepository;

    public InitService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Init()
    {
        using var scope = _serviceProvider.CreateScope();
        _groupRepository = scope.ServiceProvider.GetRequiredService<IGroupRepository>();
        var initDiets = scope.ServiceProvider.GetRequiredService<InitDiets>();

        var groups = await _groupRepository.GetAllGroupsAsync();

        foreach (var group in groups)
        {
            await initDiets.Init(group);
        }
    }
}