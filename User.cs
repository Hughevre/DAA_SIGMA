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
                if (!Senders.Any(x => x.RemoteEndPoint == newRemote))
                {
                    Sender connection = new Sender(newRemote);
                    connection.Connect();
                    Senders.Add(connection);
                }
                else
                    UI.Print("You have been already connected to that host");
            }
            catch (Exception ex)
            {
                UI.Print(ex.Message);
            }
        }

        private static void ReceiveMessage(IPEndPoint endPoint) => UI.Print(Encoding.ASCII.GetString(Receiver.ReceiveFrom(endPoint).Dequeue()));

        public static void Send(IPAddress otherAddress, int otherPort, byte[] message)
        {
            var sender = Senders.FirstOrDefault(x => x.RemoteEndPoint.Address.ToString() == otherAddress.ToString() && x.RemoteEndPoint.Port == otherPort);
            if (sender != null)
                sender.Send(message);
            else
                UI.Print("You cannot send message to disconnected host");
        }
    }
}
