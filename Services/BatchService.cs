using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FFA.Services;

namespace FFA.Services;

public class BatchService : BackgroundService
{
    private readonly ILogger<BatchService> _logger;
    private readonly IServiceProvider _services;

    public BatchService(ILogger<BatchService> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // calculate next UTC midnight
        var now = DateTime.UtcNow;
        var nextRun = now.Date.AddDays(1); // next 00:00 UTC

        while (!stoppingToken.IsCancellationRequested)
        {
            now = DateTime.UtcNow;

            if (now >= nextRun)
            {
                await Run24HourTasks(stoppingToken);
                nextRun = nextRun.AddDays(1);
            }

            // check every 30 seconds
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task Run10MinTasks(CancellationToken ct)
    {
    }

    private async Task Run60MinTasks(CancellationToken ct)
    {
    }

    private async Task Run24HourTasks(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        _logger.LogInformation("Running 24-hour tasks");

        // example task: daily bonus 100 gil
        var users = userService.GetAllUsers();
        foreach (var u in users)
        {
            u.Gil += 100;
            userService.UpdateUser(u);
        }

        await Task.CompletedTask;
    }
}
