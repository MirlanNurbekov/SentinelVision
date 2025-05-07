using System.Windows;
using Core.Hardware;
using Core.Interfaces;
using Core.Services;
using Core.Data;
using Microsoft.Extensions.DependencyInjection;
using System;

public partial class App : Application
{
    private ServiceProvider provider;

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();

        services.AddSingleton<PeopleRepository>();
        services.AddSingleton<IFaceRecognitionService, FaceRecognitionService>();
        services.AddSingleton<ICameraController>(sp => new NetworkCameraController("192.168.0.100"));
        services.AddSingleton<IDoorController>(sp => new SerialDoorController("COM3"));
        services.AddSingleton<IElevatorController>(sp => new TcpElevatorController("http://192.168.0.200"));
        services.AddSingleton<IEventBus>(sp => new EventBusService("localhost"));

        services.AddSingleton<CctvService>();
        services.AddSingleton<DoorControlService>();
        services.AddSingleton<ElevatorTrackingService>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<Views.MainWindow>();

        provider = services.BuildServiceProvider();
        var win = provider.GetRequiredService<Views.MainWindow>();
        win.Show();
    }
}