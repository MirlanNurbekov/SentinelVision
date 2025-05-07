public interface IDeviceController<TStatus>
{
    Task<(bool success, TStatus status)> ExecuteCommandAsync(string command, CancellationToken ct);
}
```

## Infrastructure/HardwareAdapters Example
```csharp
public class RtsptCameraController : ICameraController, IHealthCheck
{
    private readonly ILogger<RtsptCameraController> _logger;
    private readonly string _uri;
    private CancellationTokenSource _cts;

    public RtsptCameraController(IConfiguration config, ILogger<RtsptCameraController> logger)
    {
        _uri = config["Cameras:Main:RtspUri"];
        _logger = logger;
    }

    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        try
        {
            var rtspClient = new RtspClient(_uri);
            await foreach (var frame in rtspClient.StreamFramesAsync(_cts.Token))
            {
                FrameReceived?.Invoke(this, frame);
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "RTSP stream error"); }
    }

    public async Task StopAsync() => _cts.Cancel();
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context)
        => await Task.FromResult(HealthCheckResult.Healthy());
    public event EventHandler<byte[]> FrameReceived;
}