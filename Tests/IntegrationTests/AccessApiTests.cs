using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Services.API;
using Infrastructure.Persistence;
using Core.Interfaces;
using Core.Models;
using Services.API.Controllers;

namespace Tests.IntegrationTests
{
    public class AccessApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly InMemoryEventBus _eventBus;

        public AccessApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            _eventBus = factory.Services.GetRequiredService<IEventBus>() as InMemoryEventBus;
        }

        [Fact]
        public async Task HealthEndpoint_ReturnsOkAndHealthy()
        {
            var response = await _client.GetAsync("/health");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Healthy", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SwaggerEndpoint_ReturnsOpenApiSpec()
        {
            var response = await _client.GetAsync("/swagger/v1/swagger.json");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"openapi\"", json);
            Assert.Contains("\"paths\"", json);
        }

        [Theory]
        [InlineData(999, 1, HttpStatusCode.Unauthorized)]
        [InlineData(1,   1, HttpStatusCode.OK)]
        public async Task PostAccessCheck_ReturnsExpectedStatusAndEmitsEvent(int userId, int deviceId, HttpStatusCode expectedStatus)
        {
            var response = await _client.PostAsJsonAsync("/api/access/check", new { userId, deviceId });
            Assert.Equal(expectedStatus, response.StatusCode);
            if (expectedStatus == HttpStatusCode.OK)
            {
                var evt = _eventBus.PublishedEvents.OfType<AccessEvent>().FirstOrDefault();
                Assert.NotNull(evt);
                Assert.Equal(userId,   evt.UserId);
                Assert.Equal(deviceId, evt.DeviceId);
                Assert.True(evt.Granted);
            }
        }

        [Fact]
        public async Task UnknownRoute_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/nonexistent");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    public class CustomWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase("TestDb"));
                services.AddSingleton<IEventBus, InMemoryEventBus>();
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
                db.Users.Add(new User { Id = 1, Name = "TestUser" });
                db.SaveChanges();
            });
        }
    }

    public class InMemoryEventBus : IEventBus
    {
        public List<object> PublishedEvents { get; } = new();
        public void Publish<T>(T @event) => PublishedEvents.Add(@event);
        public void Subscribe<T>(Action<T> handler) { }
    }
}
