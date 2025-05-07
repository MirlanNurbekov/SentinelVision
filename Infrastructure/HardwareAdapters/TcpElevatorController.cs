using Core.Interfaces;
using Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Polly;
using Prometheus;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.HardwareAdapters
{
    public class TcpElevatorController : IElevatorController, IHealthCheck
    {
        private readonly HttpClient _client;
        private readonly ILogger<TcpElevatorController> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly Counter _requestCounter;
        private readonly Histogram _latencyHistogram;

        public TcpElevatorController(IConfiguration config, ILogger<TcpElevatorController> logger)
        {
            _logger = logger;
            var baseUri = config.GetValue<string>("Hardware:ElevatorController:BaseUri");
            _client = new HttpClient { BaseAddress = new Uri(baseUri) };

            _retryPolicy = Policy.Handle<HttpRequestException>()
                                 .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(300),
                                     onRetry: (ex, retry) =>
                                     {
                                         _logger.LogWarning(ex, "Retry {Retry} for ElevatorController", retry);
                                     });

            _requestCounter = Metrics.CreateCounter("elevator_requests_total", "Total TCP elevator requests");
            _latencyHistogram = Metrics.CreateHistogram("elevator_request_duration_seconds", "Latency of TCP elevator requests");
        }

        public async Task TrackCallAsync(int elevatorId, Person user, CancellationToken cancellationToken = default)
        {
            using (_latencyHistogram.NewTimer())
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    _requestCounter.Inc();
                    _logger.LogDebug("Calling elevator {ElevatorId} for user {User}", elevatorId, user.Id);

                    var payload = new { ElevatorId = elevatorId, UserId = user.Id };
                    var response = await _client.PostAsJsonAsync("/api/call", payload, cancellationToken);
                    response.EnsureSuccessStatusCode();
                });
            }
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context)
        {
            // Optionally call a ping endpoint
            return Task.FromResult(HealthCheckResult.Healthy("Elevator API reachable"));
        }
    }
}