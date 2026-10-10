using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using ServerApp.Models;

namespace ServerApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private string imageDirectory = "Assets/ImagesForChanging";
    private readonly TcpServerModel _serverModel = new TcpServerModel();

    private List<string> _imagePaths = new();
    private int _currentImageIndex = 0;

    [ObservableProperty] private string _currentImagePath = string.Empty;
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
                CurrentImagePath = _imagePaths[0];
            }
        }
    }

    private void OnClientMessageReceived(string message)
    {
        if (_imagePaths.Count == 0) return;

        _currentImageIndex = (_currentImageIndex + 1) % _imagePaths.Count;

        CurrentImagePath = _imagePaths[_currentImageIndex];
    }

    [RelayCommand]
    private async Task StartServerAsync()
    {
        await _serverModel.StartServer(Port);
    }
}
