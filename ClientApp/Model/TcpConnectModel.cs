using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
 
namespace Controller.Models;
 
public class CommandSender
{
    public async Task SendAsync(string ip, int p)
    {
        using TcpClient tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(ip, p);
        using NetworkStream stream = tcpClient.GetStream();
        byte[] data = Encoding.UTF8.GetBytes("CHANGE\n");
        await stream.WriteAsync(data);
    }
}