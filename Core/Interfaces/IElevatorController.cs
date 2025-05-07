namespace Core.Interfaces;

using Core.Models;

public interface IElevatorController
{
    Task TrackCallAsync(int elevatorId, Person user);
}