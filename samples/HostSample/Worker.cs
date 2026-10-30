using Microsoft.Extensions.Options;
using Sample.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HostSample;

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
        // Catch the entire loop to prevent unhandled exceptions on shutdown
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var str = JsonSerializer.Serialize(_monitor.CurrentValue, _jsonSerializerOptions);
                _logger.LogInformation(str);

                await Task.Delay(5000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // This is the expected shutdown flow, not an error. Log as info.
            _logger.LogInformation("Worker stopped normally.");
        }
        catch (Exception ex)
        {
            // Log other unexpected exceptions
            _logger.LogError(ex, "Worker encountered an error.");
        }

        /*
        // Trigger every 5 seconds
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        // WaitForNextTickAsync returns false upon cancellation without throwing exceptions
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var str = JsonSerializer.Serialize(_monitor.CurrentValue, _jsonSerializerOptions);
                _logger.LogInformation(str);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker encountered an error.");
            }
        }
        */
    }
}