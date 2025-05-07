using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Prometheus;
using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.HardwareAdapters
{
    public class SerialDoorController : IDeviceController<DoorOperationResult>, IHealthCheck, IDisposable
    {
        private readonly SerialPort _port;
        private readonly ILogger<SerialDoorController> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly Counter _commandCounter;
        private readonly Histogram _latencyHistogram;

        public SerialDoorController(IConfiguration config, ILogger<SerialDoorController> logger)
        {
            _logger = logger;
            var portName = config.GetValue<string>("Hardware:DoorController:PortName");
            var baudRate = config.GetValue<int>("Hardware:DoorController:BaudRate");
            _port = new SerialPort(portName, baudRate) { ReadTimeout = 1000, WriteTimeout = 1000 };
            _port.Open();

            _retryPolicy = Policy.Handle<Exception>()
                                 .WaitAndRetryAsync(3, retry => TimeSpan.FromMilliseconds(200 * retry),
                                     onRetry: (ex, retryCount, _) =>
                                     {
                                         _logger.LogWarning(ex, "Retry {RetryCount} on SerialDoorController", retryCount);
                                     });

            _commandCounter = Metrics.CreateCounter("serial_door_commands_total", "Total number of serial door commands");
            _latencyHistogram = Metrics.CreateHistogram("serial_door_command_duration_seconds", "Latency of serial door commands");
        }

        public async Task<(bool success, DoorOperationResult status)> ExecuteCommandAsync(string command, CancellationToken cancellationToken)
        {
            using (_latencyHistogram.NewTimer())
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    _commandCounter.Inc();
                    _logger.LogDebug("Sending command: {Command}", command);

                    _port.WriteLine(command);
                    await Task.Delay(100, cancellationToken);

                    var response = _port.ReadLine();
                    var result = new DoorOperationResult
                    {
                        Success = response.StartsWith("OK"),
                        Timestamp = DateTime.UtcNow,
                        RawResponse = response
                    };

                    _logger.LogInformation("Door command result: {Result}", result.Success);
                    return (result.Success, result);
                });
            }
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context)
        {
            return Task.FromResult(
                _port.IsOpen
                    ? HealthCheckResult.Healthy("Serial port is open and operational")
                    : HealthCheckResult.Unhealthy("Serial port is closed")
            );
        }

        public void Dispose()
        {
            if (_port.IsOpen) _port.Close();
            _port.Dispose();
        }
    }

    public class DoorOperationResult
    {
        public bool Success { get; set; }
        public DateTime Timestamp { get; set; }
        public string RawResponse { get; set; }
    }
}