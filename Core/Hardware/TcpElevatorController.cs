namespace Core.Hardware;

using Core.Interfaces;
using Core.Models;
using System.Net.Http;
using System.Threading.Tasks;

public class TcpElevatorController : IElevatorController
{
    private readonly HttpClient client;

    public TcpElevatorController(string baseUri)
    {
        client = new HttpClient { BaseAddress = new Uri(baseUri) };
    }

    public async Task TrackCallAsync(int elevatorId, Person user)
    {
        var payload = new { elevatorId, userId = user.Id };
        await client.PostAsJsonAsync("/api/elevator/call", payload);
    }
}
