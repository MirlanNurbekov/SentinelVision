namespace Core.Services;

using Core.Interfaces;
using System;

public class CctvService
{
    private readonly ICameraController camera;

    public CctvService(ICameraController camera)
    {
        this.camera = camera;
        camera.FrameReceived += OnFrame;
    }

    public event EventHandler<byte[]> FrameReceived;

    public void Start(int cameraId) => camera.Start(cameraId);
    public void Stop(int cameraId) => camera.Stop(cameraId);

    private void OnFrame(object s, byte[] frame) => FrameReceived?.Invoke(this, frame);
}
