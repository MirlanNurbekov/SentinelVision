namespace Core.Interfaces;

using Core.Models;

public interface IFaceRecognitionService
{
    Task<Person> RecognizeAsync(byte[] frame);
}
