using System.Threading.Tasks;
using Xunit;
using Core.Hardware;

public class HardwareSimulationTests
{
    [Fact]
    public async Task SerialDoorController_UnlockAndLock_Success()
    {
        var simPort = "COM_SIM";
        var controller = new SerialDoorController(simPort);
        var unlock = await controller.UnlockAsync(1);
        var lockRes = await controller.LockAsync(1);
        Assert.True(unlock);
        Assert.True(lockRes);
    }

    [Fact]
    public async Task NetworkCameraController_ReceivesFrames()
    {
        var controller = new NetworkCameraController("127.0.0.1:9000");
        bool received = false;
        controller.FrameReceived += (_, frame) => received = true;
        controller.Start(0);
        await Task.Delay(100);
        Assert.True(received);
        controller.Stop(0);
    }
} 