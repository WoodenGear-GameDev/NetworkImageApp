using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Controller;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    public async void Button_Click(object? sender, RoutedEventArgs e)
    {
        try {
        string ip = IpBox.Text?.Trim() ?? "";
        if(ip is "")
        {
            Status.Text = "Введен неправильный ip-адрес";
            return;
        }
        string port = PortBox.Text?.Trim() ?? "";
        if(!int.TryParse(port, out int p) || (p < 1 || p > 65535))
        {
            Status.Text = "Введен неправильный порт";
            return;
        }
        SendButton.IsEnabled = false;
        Status.Text = "Подключение..";
        using TcpClient tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(ip, p);
        using NetworkStream stream = tcpClient.GetStream();
        byte[] data = Encoding.UTF8.GetBytes("CHANGE\n");
        await stream.WriteAsync(data);
        Status.Text = "Команда отправлена";

        } catch(SocketException)
        {
            Status.Text =
                "Не удалось подключиться. Проверьте IP, порт и сеть.";
        }
         catch (Exception ex)
        {
            Status.Text = $"Ошибка: {ex.Message}";
        }
        finally
        {
            SendButton.IsEnabled = true;
        }
    }

}
