using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace Controller.Models;
public class CommandSender
{
public NetworkStream stream;
private TcpClient? tcpClient;
public async Task Connect(string ip, int p)
{
    tcpClient = new TcpClient();
    await tcpClient.ConnectAsync(ip, p);
    stream = tcpClient.GetStream();
}
public async Task Send(NetworkStream stream)
{
        byte[] data = Encoding.UTF8.GetBytes("CHANGE\n");
        await stream.WriteAsync(data);
}
public void Disconnect()
{
    stream?.Dispose();
    tcpClient?.Dispose();
}
}