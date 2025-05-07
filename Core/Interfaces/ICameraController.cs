namespace Core.Interfaces;

public interface ICameraController
{
    event EventHandler<byte[]> FrameReceived;
    void Start(int cameraId);
    void Stop(int cameraId);
}