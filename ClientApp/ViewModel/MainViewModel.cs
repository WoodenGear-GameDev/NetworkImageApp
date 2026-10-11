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
    private bool _isSendEnabled = true;

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

    public bool IsSendEnabled
    {
        get => _isSendEnabled;
        set => SetField(ref _isSendEnabled, value);
    }

    public ICommand SendCommand { get; }
    public ICommand Connect{get;}

    public MainWindowViewModel()
    {
        Connect = new AsyncRelayCommand(SendAsync);
        SendCommand = new AsyncRelayCommand(SendAsync);
    }
    private async Task ConnectToServ()
    {
        
    }

    private async Task SendAsync()
    {
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
        IsSendEnabled = false;
        Status = "Подключение..";
        await _sender.SendAsync(ip, p);
        Status = "Команда отправлена";

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
            IsSendEnabled = true;
        }
    }
}