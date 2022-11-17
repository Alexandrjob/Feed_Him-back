using TaskTrackerCat.Infrastructure.InitServices;

namespace TaskTrackerCat.Infrastructure.HostedServices;

public class TimedHostedService : IHostedService, IDisposable
{
    private Timer? _timer = null;
    private readonly InitService _initService;

    public TimedHostedService(InitService initService)
    {
        _initService = initService;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(CheckDataBase, null, TimeSpan.Zero, TimeSpan.FromDays(28));
        return Task.CompletedTask;
    }

    private async void CheckDataBase(object? state)
    {
        //Правильно так?
        var daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
        var nextDaysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.AddMonths(1).Month);
        _timer.Change(TimeSpan.FromDays(daysInMonth - 2), TimeSpan.FromDays(nextDaysInMonth - 2));

        try
        {
            await _initService.Init();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}