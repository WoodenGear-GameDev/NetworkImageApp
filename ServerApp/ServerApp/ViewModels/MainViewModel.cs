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
    [ObservableProperty]
    private string _errorMessage = string.Empty;


    public MainViewModel()
    {
        LoadImagesForChanging();

        _serverModel.OnMessageReceived += OnClientMessageReceived;
        _serverModel.OnErrorOccurred += OnServerErrorOccurred;
    }

    private void OnServerErrorOccurred(string errorText)
    {
        NewConsoleMessage(errorText);
    }

    private void NewConsoleMessage(string message)
    {
        if (ErrorMessage.Length > 10000)
        {
            int index = ErrorMessage.IndexOf('\n');
            if (index != -1)
            {
                ErrorMessage = ErrorMessage.Substring(index + 1);
            }
        }
        ErrorMessage = $"{ErrorMessage}\n{message}";
    }

    [RelayCommand]
    private void CleanErrorHistory()
    {
        ErrorMessage = string.Empty;
    }

    private void LoadImagesForChanging()
    {
        try
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
            else
            {
                NewConsoleMessage("Image assets folder not found!");
            }
        }
        catch (Exception ex)
        {
            NewConsoleMessage($"Error loading images: {ex.Message}");
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
                NewConsoleMessage("Invalid port! Enter a number between 1 and 65535.");
                return;
            }

            _serverModel.StartServer(portNumber);
        }
        else
        {
            _serverModel.StopServer();
        }

        UpdateUiProperties();
    }

    private void UpdateUiProperties()
    {
        OnPropertyChanged(nameof(IsServerRunning));
        OnPropertyChanged(nameof(StartServerButtonText));
        OnPropertyChanged(nameof(PortTextboxIsReadOnly));
    }
}
