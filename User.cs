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

        static User()
        {
            Senders     = new List<Sender>();
            Receiver    = new Receiver();

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

                    MessageHeader handShake = new MessageHeader(MessageHeader.MessageType.TCPHANDSHAKE, Encoding.ASCII.GetBytes(Receiver.LocalEndPoint.Address.ToString() + "@" + Receiver.LocalEndPoint.Port.ToString()));
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

        private static void ReceiveMessage(IPEndPoint endPoint)
        {
            byte[] messageBytes = Receiver.ReceiveFrom(endPoint).Dequeue();
            MessageHeader message = new MessageHeader(messageBytes);

            switch (message.Signaling)
            {
                case MessageHeader.MessageType.TCPHANDSHAKE:
                    HandleTCPHandShake(message.Payload);
                    break;
                case MessageHeader.MessageType.POST:
                    HandlePost(endPoint, message.Payload);
                    break;
                case MessageHeader.MessageType.SIGMA:
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

        #region Handlers
        private static void HandleTCPHandShake(byte[] payload)
        {
            string remoteEndPoint    = Encoding.ASCII.GetString(payload).Replace("\0", "");
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
        #endregion
    }
}
