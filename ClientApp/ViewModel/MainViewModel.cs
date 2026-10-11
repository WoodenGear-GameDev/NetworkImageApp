using Controller.Models;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Controller.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly CommandSender _sender = new CommandSender();

    private string _ipText = "";
    private string _portText = "";
    private string _status = "Статус";
    private bool _isConnectEnabled = true;
    private bool _isSendEnabled = false;
    private bool _isDisconnectEnabled = false;

    public string IpText
    {
        get => _ipText;
        set => SetField(ref _ipText, value);
    }

    public string PortText
    {
        get => _portText;
        set => SetField(ref _portText, value);
    }

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public bool IsConnectEnabled
    {
        get => _isConnectEnabled;
        set => SetField(ref _isConnectEnabled, value);
    }

    public bool IsSendEnabled
    {
        get => _isSendEnabled;
        set => SetField(ref _isSendEnabled, value);
    }

    public bool IsDisconnectEnabled
    {
        get => _isDisconnectEnabled;
        set => SetField(ref _isDisconnectEnabled, value);
    }

    public ICommand ConnectCommand { get; }
    public ICommand SendCommand { get; }
    public ICommand DisconnectCommand { get; }

    public MainWindowViewModel()
    {
        ConnectCommand = new AsyncRelayCommand(ConnectAsync);
        SendCommand = new AsyncRelayCommand(SendAsync);
        DisconnectCommand = new AsyncRelayCommand(DisconnectAsync);
    }

    private async Task ConnectAsync()
    {
        bool connected = false;
        try {
        string ip = IpText?.Trim() ?? "";
        if(ip is "")
        {
            Status = "Введен неправильный ip-адрес";
            return;
        }
        string port = PortText?.Trim() ?? "";
        if(!int.TryParse(port, out int p) || (p < 1 || p > 65535))
        {
            Status = "Введен неправильный порт";
            return;
        }
        IsConnectEnabled = false;
        Status = "Подключение..";
        await _sender.Connect(ip, p);
        connected = true;
        Status = "Подключено";
        IsSendEnabled = true;
        IsDisconnectEnabled = true;

        } catch(SocketException)
        {
            Status =
                "Не удалось подключиться. Проверьте IP, порт и сеть.";
        }
         catch (Exception ex)
        {
            Status = $"Ошибка: {ex.Message}";
        }
        finally
        {
            if(!connected)
            {
                _sender.Disconnect();
                IsConnectEnabled = true;
            }
        }
    }

    private async Task SendAsync()
    {
        try {
        await _sender.Send(_sender.stream);
        Status = "Команда отправлена";

        } catch (Exception ex)
        {
            Status = $"Ошибка: {ex.Message}";
        }
    }

    private Task DisconnectAsync()
    {
        _sender.Disconnect();
        Status = "Отключено";
        IsConnectEnabled = true;
        IsSendEnabled = false;
        IsDisconnectEnabled = false;
        return Task.CompletedTask;
    }
}