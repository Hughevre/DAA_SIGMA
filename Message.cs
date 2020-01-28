using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS_DAA_SIGMA
{
    class Message<T> where T: System.Enum
    {
        public T        Signaling { get; private set; }
        public byte[]   Payload { get; private set; }

        private const int _signalingLength    = 32;
        private const int _payloadLength      = 2048;

        public Message(T signaling, byte[] payload)
        {
            Signaling = signaling;
            Payload = payload;
        }

        public Message(byte[] message)
        {
            try
            {
                byte[] signalingBytes   = new byte[_signalingLength];
                byte[] payloadBytes     = new byte[message.Length - _signalingLength];

                Buffer.BlockCopy(message, 0, signalingBytes, 0, _signalingLength);
                Buffer.BlockCopy(message, _signalingLength, payloadBytes, 0, message.Length - _signalingLength);

                Signaling = (T)Enum.Parse(typeof(T), Encoding.ASCII.GetString(signalingBytes).Replace("\0", string.Empty));
                Payload = payloadBytes;
            }
            catch (Exception ex)
            {
                UI.Print(ex.Message);
            }
        }

        public byte[] ConvertToBytes()
        {
            if (Payload.Length > _payloadLength)
                throw new ArgumentOutOfRangeException();

            byte[] ret = null;
            try
            {
                ret = new byte[_signalingLength + Payload.Length];
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(Signaling.ToString()), 0, ret, 0, Encoding.ASCII.GetBytes(Signaling.ToString()).Length);
                Buffer.BlockCopy(Payload, 0, ret, _signalingLength, Payload.Length);
            }
            catch (Exception ex)
            {
                UI.Print(ex.Message);
            }

            return ret;
        }
    }
}
