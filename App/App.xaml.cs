using System.Windows;
using Core.Services;
using Microsoft.Extensions.DependencyInjection;

public partial class App : Application
{
    private readonly ServiceProvider provider;

    public App()
    {
        var services = new ServiceCollection();
        services.AddSingleton<FaceRecognitionService>();
        services.AddSingleton<DoorControlService>();
        services.AddSingleton<ElevatorTrackingService>();
        services.AddSingleton<CctvService>();
        services.AddSingleton<MainWindow>();
        provider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var window = provider.GetRequiredService<MainWindow>();
        window.Show();
    }
}
