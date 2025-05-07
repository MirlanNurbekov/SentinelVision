using System.Windows;
using System.Windows.Threading;
using Core.Models;
using Core.Services;

public partial class MainWindow : Window
{
    private readonly FaceRecognitionService faceService;
    private readonly DoorControlService doorService;
    private readonly CctvService cctvService;
    private readonly DispatcherTimer timer;

    public MainWindow(FaceRecognitionService f, DoorControlService d, CctvService c)
    {
        faceService = f;
        doorService = d;
        cctvService = c;
        InitializeComponent();
        timer = new DispatcherTimer { Interval = System.TimeSpan.FromMilliseconds(100) };
        timer.Tick += OnFrame;
        timer.Start();
        OpenDoorButton.Click += (_, _) => doorService.OpenDoor(1, PeopleList.SelectedItem as Person);
    }

    private void OnFrame(object sender, System.EventArgs e)
    {
        var frame = cctvService.GetFrame(0);
        if (faceService.Recognize(frame, out Person match))
        {
            if (!PeopleList.Items.Contains(match)) PeopleList.Items.Add(match);
        }
    }
}