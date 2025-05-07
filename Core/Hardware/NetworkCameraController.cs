namespace Core.Hardware;

using Core.Interfaces;
using System.Net.Sockets;
using System;

public class NetworkCameraController : ICameraController
{
    public event EventHandler<byte[]> FrameReceived;

    private readonly TcpClient client;

    public NetworkCameraController(string endpoint)
    {
        client = new TcpClient(endpoint, 9000);
        Task.Run(() => Listen());
    }

    private void Listen()
    {
        var stream = client.GetStream();
        while(true)
        {
            var lengthBytes = new byte[4];
            stream.Read(lengthBytes);
            int length = BitConverter.ToInt32(lengthBytes);
            var buffer = new byte[length];
            stream.Read(buffer);
            FrameReceived?.Invoke(this, buffer);
        }
    }

    public void Start(int cameraId) { /* Send START signal */ }
    public void Stop(int cameraId) { /* Send STOP signal */ }
}