namespace Core.Services;

using Core.Interfaces;
using Core.Models;
using OpenCvSharp;
using System.Threading.Tasks;

public class FaceRecognitionService : IFaceRecognitionService
{
    private readonly PeopleRepository repo;

    public FaceRecognitionService(PeopleRepository repo)
    {
        this.repo = repo;
    }

    public async Task<Person> RecognizeAsync(byte[] frame)
    {
        // Complex ML inference here
        await Task.Delay(50);
        return null;
    }
}