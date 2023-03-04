using Microsoft.Extensions.Options;
using Sample.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkerServiceSample
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IOptionsMonitor<ConfigOptions> _monitor;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };


        public Worker(ILogger<Worker> logger, IOptionsMonitor<ConfigOptions> monitor)
        {
            _logger = logger;
            _monitor = monitor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var str = JsonSerializer.Serialize(_monitor.CurrentValue, _jsonSerializerOptions);
                _logger.LogInformation(str);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}