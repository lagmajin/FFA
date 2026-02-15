using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SignalR;
using FFA.Hubs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FFA.Services
{
    public class TimeWeatherHostedService : BackgroundService
    {
        private readonly TimeWeatherService _service;
        private readonly IHubContext<WorldHub> _hub;

        public TimeWeatherHostedService(TimeWeatherService service, IHubContext<WorldHub> hub)
        {
            _service = service;
            _hub = hub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var last = DateTime.UtcNow;
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var delta = now - last;
                last = now;
                // map real time to game hours: e.g., 1 real sec = 1 game minute => 60x faster
                double hours = delta.TotalSeconds / 60.0; // 60 seconds = 1 hour
                _service.Advance(hours);
                // broadcast world update
                await _hub.Clients.All.SendAsync("WorldUpdated", new { time = _service.TimeOfDay, phase = _service.Phase.ToString(), weather = _service.Weather.ToString() });
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
