using System;
using System.ComponentModel;
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
        public event Action<string>? OnErrorOccurred;

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
            try
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
            }
            catch (Exception)
            {

            }

            return "127.0.0.1";
        }

        public void StartServer(int port)
        {
            try
            {
                _tcpListener = new TcpListener(IPAddress.Any, port);
                _tcpListener.Start();

                _cts = new CancellationTokenSource();

                _ = StartListening(_tcpListener, _cts.Token);

                IsRunning = true;
            }
            catch (SocketException ex)
            {
                IsRunning = false;
                _tcpListener?.Stop();

                OnErrorOccurred?.Invoke($"Network error: port {port} is already in use or unavailable. ({ex.Message})");
            }
            catch (Exception ex)
            {
                IsRunning = false;
                OnErrorOccurred?.Invoke($"Network error: Failed to start server: {ex.Message}");
            }
        }

        public void StopServer()
        {
            try
            {
                _cts?.Cancel();
                _tcpListener?.Stop();
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Network error: Failed to stop server: {ex.Message}");
            }
            finally
            {
                IsRunning = false;
            }
        }

        public async Task StartListening(TcpListener tcpListener, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    TcpClient client = await tcpListener.AcceptTcpClientAsync(cancellationToken);

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
            catch (OperationCanceledException)
            {
                
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Error in the server background thread: {ex.Message}");
            }
            finally
            {
                IsRunning = false;
            }
        }
    }
}
