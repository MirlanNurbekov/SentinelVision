using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using App.ViewModels;
using Core.Data;
using Core.Interfaces;
using Core.Services;
using Infrastructure.HardwareAdapters;

namespace App
{
    public partial class App : Application
    {
        private static readonly IHost Host;

        static App()
        {
            Host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddEnvironmentVariables();
                })
                .ConfigureLogging(log =>
                {
                    log.ClearProviders();
                    log.AddConsole();
                })
                .ConfigureServices((ctx, services) =>
                {
                    var conf = ctx.Configuration;

                    services.AddSingleton<PeopleRepository>();
                    services.AddSingleton<IFaceRecognitionService, FaceRecognitionService>();

                    services.AddSingleton<ICameraController>(sp =>
                        new NetworkCameraController(conf["Hardware:Camera:Endpoint"]));

                    services.AddSingleton<IDoorController>(sp =>
                        new SerialDoorController(
                            conf["Hardware:Door:PortName"],
                            int.Parse(conf["Hardware:Door:BaudRate"])));

                    services.AddSingleton<IElevatorController>(sp =>
                        new TcpElevatorController(conf["Hardware:Elevator:BaseUri"]));

                    services.AddSingleton<IEventBus>(sp =>
                        new EventBusService(conf["EventBus:Host"]));

                    services.AddSingleton<CctvService>();
                    services.AddSingleton<DoorControlService>();
                    services.AddSingleton<ElevatorTrackingService>();

                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await Host.StartAsync();
            Host.Services.GetRequiredService<MainWindow>().Show();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await Host.StopAsync(TimeSpan.FromSeconds(5));
            Host.Dispose();
            base.OnExit(e);
        }
    }
}
