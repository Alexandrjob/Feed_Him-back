using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TaskTrackerCat.BLL.Services;

public class TimedHostedService : IHostedService, IDisposable
{
    private readonly InitService _initService;
    private readonly ILogger<TimedHostedService> _logger;
    private Timer? _timer;

    public TimedHostedService(ILogger<TimedHostedService> logger, InitService initService)
    {
        _logger = logger;
        _initService = initService;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Инициализация таймера генерации примов пищи");
        _timer = new Timer(CheckDataBase, null, TimeSpan.Zero, TimeSpan.FromDays(28));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    private async void CheckDataBase(object? state)
    {
        //Правильно так?
        var daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
        var nextDaysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.AddMonths(1).Month);
        _timer.Change(TimeSpan.FromDays(daysInMonth - 2), TimeSpan.FromDays(nextDaysInMonth - 2));
        _logger.LogInformation("Изменение следующего запуска таймера.Следующий запуск:{NextStart}",
            TimeSpan.FromDays(daysInMonth - 2));
        _logger.LogInformation("Изменение интервала запуска таймера.Новый интервал:{Period}",
            TimeSpan.FromDays(nextDaysInMonth - 2));

        try
        {
            _logger.LogInformation("Запуск генерации приемов пищи.");
            await _initService.Init();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Произошла ошибка во время генерации приемов пищи.");
            throw;
        }
    }
}