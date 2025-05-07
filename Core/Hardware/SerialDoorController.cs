namespace Core.Hardware;

using Core.Interfaces;
using System.IO.Ports;
using System.Threading.Tasks;

public class SerialDoorController : IDoorController
{
    private readonly SerialPort port;

    public SerialDoorController(string portName)
    {
        port = new SerialPort(portName, 9600);
        port.Open();
    }

    public async Task<bool> UnlockAsync(int doorId)
    {
        port.WriteLine($"UNLOCK:{doorId}");
        await Task.Delay(200);
        return true;
    }

    public async Task<bool> LockAsync(int doorId)
    {
        port.WriteLine($"LOCK:{doorId}");
        await Task.Delay(200);
        return true;
    }
}
