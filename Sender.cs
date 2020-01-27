using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace BUS_DAA_SIGMA
{
    class Sender
    {
        public IPEndPoint LocalEndPoint { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }

        private TcpClient       _sender;
        private NetworkStream   _stream;

        public Sender(IPEndPoint other)
        {
            LocalEndPoint    = new IPEndPoint(IPAddress.Parse("127.0.0.1"), TCPBox.SearchFreeTCPPort());
            RemoteEndPoint   = other;

            _sender = new TcpClient(LocalEndPoint);
        }

        public void Connect()
        {
            if (!_sender.Connected)
            {
                try
                {
                    _sender.Connect(RemoteEndPoint);

                    _stream = _sender.GetStream();
                }
                catch (Exception ex)
                {
                    UI.Print(ex.Message);
                }
            }
            else
                UI.Print("You have already been connected to that end point");
        }

        public void Disconnect()
        {
            if (_sender.Connected)
            {
                _stream.Close();
                _sender.Close();
            }
            else
                UI.Print("You are not connected to that host");
        }

        public void Send(byte[] message)
        {
            if (_sender.Connected)
            {
                if (_stream.CanWrite)
                {
                    try
                    {
                        _stream.Write(message, 0, message.Length);
                    }
                    catch (Exception ex)
                    {
                        UI.Print(ex.Message);
                    }
                }
                else
                    UI.Print("Writing to the host is illegal");
            }
            else
                UI.Print("You are not connected to the remote host");
        }
    }
}
