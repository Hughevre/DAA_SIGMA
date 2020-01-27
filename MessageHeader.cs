using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BUS_DAA_SIGMA
{
    class MessageHeader
    {
        public enum MessageType
        {
            TCPHANDSHAKE,
            POST
        }

        public MessageType      Signaling { get; private set; }
        public byte[]           Payload { get; private set; }

        private const int       _signalingLength    = 32;
        private const int       _payloadLength      = 2048;

        public MessageHeader(MessageType signaling, byte[] payload)
        {
            Signaling   = signaling;
            Payload     = payload;
        }

        public MessageHeader(byte[] message)
        {
            try
            {
                byte[] signalingBytes = new byte[_signalingLength];
                byte[] payloadBytes = new byte[message.Length - _signalingLength];

                Buffer.BlockCopy(message, 0, signalingBytes, 0, _signalingLength);
                Buffer.BlockCopy(message, _signalingLength, payloadBytes, 0, message.Length - _signalingLength);

                Signaling = (MessageType)Enum.Parse(typeof(MessageType), Encoding.ASCII.GetString(signalingBytes).Replace("\0", string.Empty));
                Payload = payloadBytes;
            }
            catch (Exception ex)
            {
                UI.Print(ex.Message);
            }
        }

        public byte[] ConvertToBytes()
        {
            byte[] ret = new byte[_signalingLength + Payload.Length];
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(Signaling.ToString()), 0, ret, 0, Encoding.ASCII.GetBytes(Signaling.ToString()).Length);
            Buffer.BlockCopy(Payload, 0, ret, _signalingLength, Payload.Length);
            return ret;
        }
    }
}
