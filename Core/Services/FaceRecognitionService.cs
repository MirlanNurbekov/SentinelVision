using Core.Models;

namespace Core.Services;

public class FaceRecognitionService
{
    public bool Recognize(byte[] frame, out Person match)
    {
        match = null;
        return false;
    }
}
