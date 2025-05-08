using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Core.Interfaces;
using Infrastructure.Notifications;
using Services.Notifications;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        // Register each provider
        services.AddSingleton<INotificationService, EmailNotificationService>();
        services.AddSingleton<INotificationService, SlackNotificationService>();
        // Composite will fan-out to all INotificationService
        services.AddSingleton<CompositeNotificationService>();
        // Background worker that polls for pending alerts
        services.AddHostedService<NotificationWorker>();
    })
    .Build();

await host.RunAsync();
