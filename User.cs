using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace BUS_DAA_SIGMA
{
    static class User
    {
        public static List<Sender>  Senders { get; private set; }
        public static Receiver      Receiver { get; private set; }

        private static List<Sigma>  _pendingSigmaTasks;
        private static object       _lockPendingSigmas;

        static User()
        {
            Senders             = new List<Sender>();
            Receiver            = new Receiver();
            _pendingSigmaTasks  = new List<Sigma>();
            _lockPendingSigmas  = new object();

            Receiver.MessageCameTriggered += ReceiveMessage;
            Receiver.Listen();
        }

        public static void Connect(IPAddress otherAddress, int otherPort)
        {
            try
            {
                IPEndPoint newRemote = new IPEndPoint(otherAddress, otherPort);
                if (!Senders.Any(x => x.RemoteEndPoint.Address.ToString() == otherAddress.ToString() && x.RemoteEndPoint.Port.ToString() == otherPort.ToString()))
                {
                    Sender connection = new Sender(newRemote);
                    connection.Connect();
                    Senders.Add(connection);

                    Message<MessageHeader.BasicType> handShake = new Message<MessageHeader.BasicType>(MessageHeader.BasicType.TCPHANDSHAKE, Encoding.ASCII.GetBytes(Receiver.LocalEndPoint.Address.ToString() + "@" + Receiver.LocalEndPoint.Port.ToString()));
                    Send(otherAddress, otherPort, handShake.ConvertToBytes());
                }
                else
                    UI.Print("You have been already connected to that host");
            }
            catch (Exception ex)
            {
                UI.Print(ex.Message);
            }
        }

        public static void Disconnect()
        {
            //TO DO
        }

        private static async Task ReceiveMessage(IPEndPoint endPoint)
        {
            byte[] messageBytes                         = Receiver.ReceiveFrom(endPoint).Dequeue();
            Message<MessageHeader.BasicType> message    = new Message<MessageHeader.BasicType>(messageBytes);

            switch (message.Signaling)
            {
                case MessageHeader.BasicType.TCPHANDSHAKE:
                    HandleTCPHandShake(message.Payload);
                    break;
                case MessageHeader.BasicType.POST:
                    HandlePost(endPoint, message.Payload);
                    break;
                case MessageHeader.BasicType.SIGMA:
                    await HandleSigma(endPoint);
                    break;
                default:
                    UI.Print("Unrecognized signaling message");
                    break;
            }
        }

        public static void Send(IPAddress otherAddress, int otherPort, byte[] message)
        {
            Sender sender = Senders.FirstOrDefault(x => x.RemoteEndPoint.Address.ToString() == otherAddress.ToString() && x.RemoteEndPoint.Port == otherPort);
            if (sender != null)
                sender.Send(message);
            else
                UI.Print("You cannot send message to disconnected host");
        }

        public static void BeginKeyExchange(IPAddress otherAddress, int otherPort)
        {
            Sender sender = Senders.FirstOrDefault(x => x.RemoteEndPoint.Address.ToString() == otherAddress.ToString() && x.RemoteEndPoint.Port == otherPort);
            if (sender.SymmetricKey == null)
            {
                var sigma = new Sigma(sender);
                Send(otherAddress, otherPort, 
                    new Message<MessageHeader.BasicType>(MessageHeader.BasicType.SIGMA, 
                    new Message<MessageHeader.SigmaType>(MessageHeader.SigmaType.PHello, sigma.GetPHello()).
                    ConvertToBytes()).
                    ConvertToBytes());
                _pendingSigmaTasks.Add(sigma);
            }
            else
                UI.Print("You have already exchanged the key");
        }
        #region Handlers
        private static void HandleTCPHandShake(byte[] payload)
        {
            string remoteEndPoint    = Encoding.ASCII.GetString(payload).Replace("\0", string.Empty);
            string[] splitedEndPoint = remoteEndPoint.Split('@');
            try
            {
                Connect(IPAddress.Parse(splitedEndPoint[0]), int.Parse(splitedEndPoint[1]));
            }
            catch (Exception ex)
            {
                UI.Print(ex.Message);
            }
        }

        private static void HandlePost(IPEndPoint other, byte[] payload) => UI.Print($"[{other.Address}:" + $"{other.Port}]" + Encoding.ASCII.GetString(payload).Replace("\0", ""));

        private static Task HandleSigma(IPEndPoint other)
        {
            return Task.Run(async () => 
            {
                lock (_lockPendingSigmas) _pendingSigmaTasks.FirstOrDefault(x => x._connection.RemoteEndPoint.Address.ToString() == other.Address.ToString()
                && x._connection.RemoteEndPoint.Port.ToString() == other.Port.ToString()).DispatchKeyExchange();
            });
        }
        #endregion
    }
}
