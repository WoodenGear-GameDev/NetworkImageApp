using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ServerApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly TcpServerModel _serverModel = new TcpServerModel();

    private string imageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "ImagesForChanging");
    private List<string> _imagePaths = new();
    private int _currentImageIndex = 0;
    [ObservableProperty] private IImage? _currentImage;

    [ObservableProperty] private string _port = "7777";
    public string ServerIp => _serverModel.ServerIp;
    public bool IsServerRunning => _serverModel.IsRunning;

    public string StartServerButtonText => IsServerRunning ? "Close" : "Open";
    public bool PortTextboxIsReadOnly => IsServerRunning;

    public MainViewModel()
    {
        LoadImagesForChanging();

        _serverModel.OnMessageReceived += OnClientMessageReceived;
    }

    private void LoadImagesForChanging()
    {
        if (Directory.Exists(imageDirectory))
        {
            _imagePaths = new List<string>(Directory.GetFiles(imageDirectory, "*.png"));
            if (_imagePaths.Count > 0)
            {
                string filePath = _imagePaths[0];
                CurrentImage = new Bitmap(filePath);
            }
        }
    }

    private void OnClientMessageReceived(string message)
    {
        if (_imagePaths.Count == 0) return;

        _currentImageIndex = (_currentImageIndex + 1) % _imagePaths.Count;

        string filePath = _imagePaths[_currentImageIndex];
        CurrentImage = new Bitmap(filePath);
    }

    [RelayCommand]
    private async Task ToggleServer()
    {
        if (!IsServerRunning)
        {
            if (!int.TryParse(Port, out int portNumber) || portNumber < 1 || portNumber > 65535)
            {
                return;
            }

            _serverModel.StartServer(portNumber);
        }
        else
        {
            _serverModel.StopServer();
        }

        OnPropertyChanged(nameof(IsServerRunning));
        OnPropertyChanged(nameof(StartServerButtonText));
        OnPropertyChanged(nameof(PortTextboxIsReadOnly));
    }
}
