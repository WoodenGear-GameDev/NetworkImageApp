using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ServerApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private string imageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "ImagesForChanging");
    private readonly TcpServerModel _serverModel = new TcpServerModel();

    private List<string> _imagePaths = new();
    private int _currentImageIndex = 0;
    [ObservableProperty] private IImage? _currentImage;
    [ObservableProperty] private string _serverIp = string.Empty;

    [ObservableProperty] private int _port = 7777;

    public MainViewModel()
    {
        LoadImagesForChanging();
        ServerIp = GetLocalIpAddress();

        _serverModel.OnMessageReceived += OnClientMessageReceived;
    }

    private string GetLocalIpAddress()
    {
        string hostName = Dns.GetHostName();

        var hostEntry = Dns.GetHostEntry(hostName);

        foreach (var ip in hostEntry.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        return "127.0.0.1";
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
    private async Task StartServerAsync()
    {
        await _serverModel.StartServer(Port);
    }
}
