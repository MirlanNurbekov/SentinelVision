namespace Core.Services;

using Core.Interfaces;
using Core.Models;
using System.Threading.Tasks;

public class DoorControlService
{
    private readonly IDoorController controller;

    public DoorControlService(IDoorController controller)
    {
        this.controller = controller;
    }

    public Task<bool> UnlockAsync(int doorId, Person user) => controller.UnlockAsync(doorId);
}
