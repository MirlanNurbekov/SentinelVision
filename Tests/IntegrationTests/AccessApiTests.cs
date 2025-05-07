using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class AccessApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AccessApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var res = await _client.GetAsync("/health");
        res.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AccessController_ReturnsUnauthorized_ForUnknownUser()
    {
        var res = await _client.PostAsJsonAsync("/api/access/check", new { userId = 999, deviceId = 1 });
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, res.StatusCode);
    }
}