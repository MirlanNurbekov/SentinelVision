using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Services.API;                   // Program entry point
using Infrastructure.Persistence;      // AppDbContext
using Core.Models;                     // User entity

namespace Tests.IntegrationTests
{
    public class AccessApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AccessApiTests(WebApplicationFactory<Program> factory)
        {
            // Replace DbContext with in-memory for testing and seed data
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase("TestDb"));

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    context.Database.EnsureCreated();
                    context.Users.Add(new User { Id = 1, Name = "TestUser" });
                    context.SaveChanges();
                });
            }).CreateClient();
        }

        [Fact]
        public async Task HealthEndpoint_ReturnsOkAndHealthyContent()
        {
            var response = await _client.GetAsync("/health");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Healthy", content);
        }

        [Fact]
        public async Task SwaggerEndpoint_ReturnsSwaggerJson()
        {
            var response = await _client.GetAsync("/swagger/v1/swagger.json");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"swagger\"", json);
        }

        [Theory]
        [InlineData(999, 42, HttpStatusCode.Unauthorized)]
        [InlineData(1, 42, HttpStatusCode.OK)]
        public async Task CheckEndpoint_ReturnsExpectedStatus(int userId, int deviceId, HttpStatusCode expectedStatus)
        {
            var payload = new { UserId = userId, DeviceId = deviceId };
            var response = await _client.PostAsJsonAsync("/api/access/check", payload);
            Assert.Equal(expectedStatus, response.StatusCode);
        }
    }
}