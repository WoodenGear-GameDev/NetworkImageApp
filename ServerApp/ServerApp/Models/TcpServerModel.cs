using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp.Models
{
    internal class TcpServerModel
    {
        public event Action<string>? OnMessageReceived;

        public async Task StartServer(int port)
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, port);

            tcpListener.Start();

            while (true)
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();

                using NetworkStream stream = client.GetStream();

                byte[] rawMessage = new byte[32];
                int byteAmount = await stream.ReadAsync(rawMessage, 0, rawMessage.Length);

                if (byteAmount > 0)
                {
                    string message = Encoding.UTF8.GetString(rawMessage, 0, byteAmount);
                    OnMessageReceived?.Invoke(message);
                }
            }
        }
    }
}
