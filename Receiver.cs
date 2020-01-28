using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace BUS_DAA_SIGMA
{
    class Receiver
    {
        public IPEndPoint               LocalEndPoint { get; private set; }
        private Dictionary<IPEndPoint, Queue<byte[]>> _clientBuffer;

        private TcpListener             _listener;
        private readonly int            _serverBackLog;

        private List<Task>              _pendingConnections;
        private readonly object         _clientLock;
        private readonly object         _bufferLock;

        public delegate Task            RcvHandlerAsync(IPEndPoint e);
        public event RcvHandlerAsync    MessageCameTriggered;

        public Receiver()
        {
            LocalEndPoint       = new IPEndPoint(IPAddress.Parse("127.0.0.1"), TCPBox.SearchFreeTCPPort());
            _clientBuffer       = new Dictionary<IPEndPoint, Queue<byte[]>>();
            _serverBackLog      = 32;
            _pendingConnections = new List<Task>();
            _clientLock         = new object();
            _bufferLock         = new object();
        }

        public Task Listen()
        {
            return Task.Run(async () =>
            {
                _listener = TcpListener.Create(LocalEndPoint.Port);

                _listener.Start(_serverBackLog);
                while (true)
                {
                    var clientTCP = await _listener.AcceptTcpClientAsync();
                    var task = RegisterConnection(clientTCP);
                    if (task.IsFaulted)
                        await task;
                }
            });
        }

        public Queue<byte[]> ReceiveFrom(IPEndPoint other) => GetOrCreateQueue(other);

        private async Task RegisterConnection(TcpClient client)
        {
            var connectionTask = HandleConnection(client);
            lock (_clientLock) _pendingConnections.Add(connectionTask);

            try
            {
                await connectionTask;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                lock (_clientLock) _pendingConnections.Remove(connectionTask);
            }
        }

        private Queue<byte[]> GetOrCreateQueue(IPEndPoint client)
        {
            Queue<byte[]> queue;
            if (!_clientBuffer.TryGetValue(client, out queue))
                queue = new Queue<byte[]>();

            return queue;
        }

        private async Task OnMessageCame(IPEndPoint e) => await MessageCameTriggered?.Invoke(e);

        private Task HandleConnection(TcpClient client)
        {
            return Task.Run(async () => 
            {
                using (var stream = client.GetStream())
                {
                    while (client.Connected)
                    {
                        do
                        {
                            var buffer = new byte[4096];
                            try
                            {
                                var byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);

                                var remoteEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;

                                var queue = GetOrCreateQueue(remoteEndPoint);
                                queue.Enqueue(buffer);

                                lock(_bufferLock) _clientBuffer[(IPEndPoint)client.Client.RemoteEndPoint] = queue;

                                await OnMessageCame(remoteEndPoint);
                            }
                            catch (Exception ex)
                            {
                                UI.Print(ex.Message);
                            }
                        } while (stream.DataAvailable);
                    }
                }
                client.Close();
            });
        }
    }
}