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
        public event Action<string>? OnErrorOccurred;
        public event Action<string>? OnMessageLogged;

        public bool IsRunning { get; private set; }
        private TcpListener? _tcpListener;
        private TcpClient? _currentClient;
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

                _ = StartListeningForClient(_tcpListener);

                IsRunning = true;

                OnMessageLogged?.Invoke($"Server started.");
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
                if(_currentClient != null)
                {
                    _currentClient.Close();
                    _currentClient = null;

                    OnMessageLogged?.Invoke($"The client disconnected.");
                }
                OnMessageLogged?.Invoke($"Server stopped.");
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

        public async Task StartListeningForClient(TcpListener tcpListener)
        {
            _cts = new CancellationTokenSource();
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    _currentClient = await tcpListener.AcceptTcpClientAsync(_cts.Token);
                    OnMessageLogged?.Invoke($"The client has connected.");
                    await StartListeningClientSignals(_currentClient, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnErrorOccurred?.Invoke($"Client connection error: {ex.Message}");
                }
            }
        }

        public async Task StartListeningClientSignals(TcpClient client, CancellationToken cancellationToken)
        {
            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        byte[] rawMessage = new byte[32];
                        int byteAmount = await stream.ReadAsync(rawMessage, 0, rawMessage.Length);

                        if (byteAmount == 0)
                        {
                            OnMessageLogged?.Invoke($"The client disconnected.");
                            _currentClient = null;
                            break;
                        }

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
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                OnErrorOccurred?.Invoke($"Client communication error: {ex.Message}");
            }
        }
    }
}
