using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Xunit;
using Infrastructure.HardwareAdapters;
using Core.Models;

public class HardwareSimulationTests : IDisposable
{
    private readonly SerialDoorController _doorController;
    private readonly NetworkCameraController _cameraController;
    private readonly TcpElevatorController _elevatorController;
    private readonly CancellationTokenSource _cts;

    public HardwareSimulationTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string,string>
            {
                ["Hardware:DoorController:PortName"] = "COM_SIM",
                ["Hardware:DoorController:BaudRate"] = "9600",
                ["Hardware:CameraController:Endpoint"] = "127.0.0.1",
                ["Hardware:ElevatorController:BaseUri"] = "http://localhost"
            })
            .Build();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

        _doorController = new SerialDoorController(config, loggerFactory.CreateLogger<SerialDoorController>());
        _cameraController = new NetworkCameraController(config, loggerFactory.CreateLogger<NetworkCameraController>());
        _elevatorController = new TcpElevatorController(config, loggerFactory.CreateLogger<TcpElevatorController>());

        _cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task SerialDoorController_UnlockAndLock_SuccessStatus()
    {
        var (unlockSuccess, unlockResult) = await _doorController.ExecuteCommandAsync("UNLOCK:1", _cts.Token);
        Assert.True(unlockSuccess);
        Assert.StartsWith("OK", unlockResult.RawResponse);
        Assert.InRange(unlockResult.Timestamp, DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow);

        var (lockSuccess, lockResult) = await _doorController.ExecuteCommandAsync("LOCK:1", _cts.Token);
        Assert.True(lockSuccess);
        Assert.Equal("OK_LOCK", lockResult.RawResponse);
    }

    [Fact]
    public async Task SerialDoorController_CheckHealth_UnhealthyAfterDispose()
    {
        _doorController.Dispose();
        var health = await _doorController.CheckHealthAsync(new HealthCheckContext());
        Assert.False(health.Status == HealthStatus.Healthy);
    }

    [Fact]
    public async Task NetworkCameraController_FrameReceptionAndHealth()
    {
        bool frameReceived = false;
        _cameraController.FrameReceived += (_, frame) => frameReceived = frame.Length > 0;

        _cameraController.Start(0);
        await Task.Delay(300);
        Assert.True(frameReceived);

        var health = await _cameraController.CheckHealthAsync(new HealthCheckContext());
        Assert.Equal(HealthStatus.Healthy, health.Status);
    }

    [Fact]
    public async Task TcpElevatorController_TrackCall_NoException()
    {
        var user = new Person { Id = 42, Name = "TestUser" };
        await _elevatorController.TrackCallAsync(2, user, _cts.Token);
    }

    [Fact]
    public async Task TcpElevatorController_HealthCheck_ReturnsHealthy()
    {
        var health = await _elevatorController.CheckHealthAsync(new HealthCheckContext());
        Assert.Equal(HealthStatus.Healthy, health.Status);
    }

    public void Dispose()
    {
        _doorController.Dispose();
        _cameraController.Dispose();
        _cts.Cancel();
    }
}