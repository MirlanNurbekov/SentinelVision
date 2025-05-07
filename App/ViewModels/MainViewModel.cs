namespace App.ViewModels;

using Core.Models;
using Core.Services;
using Core.Interfaces;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public class MainViewModel
{
    public ObservableCollection<Person> DetectedPeople { get; } = new();
    public ObservableCollection<string> Logs { get; } = new();

    private readonly CctvService cctv;
    private readonly IFaceRecognitionService faceService;
    private readonly DoorControlService doorService;
    private readonly ElevatorTrackingService elevatorService;
    private readonly IEventBus bus;

    public MainViewModel(CctvService cctv, IFaceRecognitionService faceService,
                         DoorControlService doorService, ElevatorTrackingService elevatorService,
                         IEventBus bus)
    {
        this.cctv = cctv;
        this.faceService = faceService;
        this.doorService = doorService;
        this.elevatorService = elevatorService;
        this.bus = bus;

        cctv.FrameReceived += async (_, frame) => await ProcessFrame(frame);
        cctv.Start(1);

        bus.Subscribe<string>(msg => Logs.Add(msg));
    }

    private async Task ProcessFrame(byte[] frame)
    {
        var person = await faceService.RecognizeAsync(frame);
        if (person != null)
        {
            DetectedPeople.Add(person);
            await doorService.UnlockAsync(1, person);
            await elevatorService.TrackCallAsync(1, person);
            bus.Publish($"Access granted to {person.Name}");
        }
    }
}
