namespace Core.Services;

using Core.Interfaces;
using Core.Models;
using System.Threading.Tasks;

public class ElevatorTrackingService
{
    private readonly IElevatorController controller;

    public ElevatorTrackingService(IElevatorController controller)
    {
        this.controller = controller;
    }

    public Task TrackCallAsync(int elevatorId, Person user) => controller.TrackCallAsync(elevatorId, user);
}
