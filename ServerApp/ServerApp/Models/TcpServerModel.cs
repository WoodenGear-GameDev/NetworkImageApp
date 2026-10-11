using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerApp.Models
{
    internal class TcpServerModel
    {
        public event Action<string>? OnMessageReceived;

        public bool IsRunning { get; private set; }
        private TcpListener? _tcpListener;
        private CancellationTokenSource? _cts;

        public string ServerIp { get; private set; } = string.Empty;

        public TcpServerModel()
        {
            ServerIp = GetLocalIpAddress();
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

        public void StartServer(int port)
        {
            _tcpListener = new TcpListener(IPAddress.Any, port);
            _tcpListener.Start();

            _cts = new CancellationTokenSource();

            _ = StartListening(_tcpListener, _cts.Token);

            IsRunning = true;
        }

        public void StopServer()
        {
            _cts?.Cancel();
            _tcpListener?.Stop();

            IsRunning = false;
        }

        public async Task StartListening(TcpListener tcpListener, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
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
