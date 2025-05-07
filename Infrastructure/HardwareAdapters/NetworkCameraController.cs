using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Polly;
using Prometheus;
using System;
using System.Buffers;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.HardwareAdapters
{
    public class NetworkCameraController : ICameraController, IHealthCheck, IDisposable
    {
        private readonly string _endpoint;
        private readonly ILogger<NetworkCameraController> _logger;
        private readonly AsyncPolicy _reconnectPolicy;
        private readonly Gauge _connectionGauge;
        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;

        public event EventHandler<byte[]> FrameReceived;

        public NetworkCameraController(IConfiguration config, ILogger<NetworkCameraController> logger)
        {
            _logger = logger;
            _endpoint = config.GetValue<string>("Hardware:CameraController:Endpoint");
            _connectionGauge = Metrics.CreateGauge("camera_connection_status", "1 if connected, 0 if disconnected");

            _reconnectPolicy = Policy.Handle<Exception>()
                                     .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(5),
                                         onRetry: (ex, _) => _logger.LogWarning(ex, "Reconnecting to camera at {Endpoint}", _endpoint));

            ConnectAsync();
        }

        private async Task ConnectAsync()
        {
            await _reconnectPolicy.ExecuteAsync(async () =>
            {
                _client?.Close();
                _client = new TcpClient();
                await _client.ConnectAsync(_endpoint, 9000);
                _stream = _client.GetStream();
                _connectionGauge.Set(1);
                _logger.LogInformation("Connected to camera at {Endpoint}", _endpoint);

                _cts = new CancellationTokenSource();
                _ = Task.Run(() => ReceiveLoop(_cts.Token));
            });
        }

        private async Task ReceiveLoop(CancellationToken ct)
        {
            var lengthBuffer = new byte[4];
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await _stream.ReadAsync(lengthBuffer, ct);
                    var length = BitConverter.ToInt32(lengthBuffer, 0);
                    var buffer = ArrayPool<byte>.Shared.Rent(length);

                    try
                    {
                        var read = await _stream.ReadAsync(buffer, 0, length, ct);
                        FrameReceived?.Invoke(this, buffer.AsSpan(0, read).ToArray());
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "Camera stream error, reconnecting");
                    _connectionGauge.Set(0);
                    await ConnectAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in ReceiveLoop");
                    break;
                }
            }
        }

        public void Start(int cameraId) { /* if multiple feeds needed */ }
        public void Stop(int cameraId) => _cts.Cancel();

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context)
        {
            var healthy = _client?.Connected == true;
            return Task.FromResult(
                healthy
                    ? HealthCheckResult.Healthy("Camera connection healthy")
                    : HealthCheckResult.Unhealthy("Camera disconnected")
            );
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _stream?.Dispose();
            _client?.Close();
        }
    }
}