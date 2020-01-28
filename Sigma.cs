using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Net;

namespace BUS_DAA_SIGMA
{
    class Sigma
    {
        private readonly int    _sessionIDLength;
        private readonly int    _privatePartLength;

        public Sender           _connection;

        private BigInteger      _p; // GF prime
        private BigInteger      _g; // GF generator
        private BigInteger      _q; // GF order
        private BigInteger      _s; // Session ID
        private BigInteger      _x; // P's private part

        public Sigma(Sender connection)
        {
            _sessionIDLength    = 256;
            _privatePartLength  = 128;
            _connection         = connection;
            _p                  = BigInteger.Parse(
                CryptoBox.MODP_2048_256_p.
                Replace(" ", string.Empty).
                Replace("\r", string.Empty).
                Replace("\n", string.Empty), System.Globalization.NumberStyles.AllowHexSpecifier);
            _g                  = BigInteger.Parse(
                CryptoBox.MODP_2048_256_g.
                Replace(" ", string.Empty).
                Replace("\r", string.Empty).
                Replace("\n", string.Empty), System.Globalization.NumberStyles.AllowHexSpecifier);
            _q                  = BigInteger.Parse(
                CryptoBox.MODP_2048_256_q.
                Replace(" ", string.Empty).
                Replace("\r", string.Empty).
                Replace("\n", string.Empty), System.Globalization.NumberStyles.AllowHexSpecifier);
            _x                  = CryptoBox.GetRandomPositiveBigInteger(_privatePartLength);
        }

        public byte[] GetPHello()
        {
            _s = CryptoBox.GetRandomPositiveBigInteger(_sessionIDLength);
            //P's private key is g^x mod p, thus it's size never...
            byte[] ret = new byte[_sessionIDLength + _p.ToByteArray().Length];
            var privateKey = BigInteger.ModPow(_g, _x, _p).ToByteArray();
            Buffer.BlockCopy(_s.ToByteArray(), 0, ret, 0, _s.ToByteArray().Length);
            Buffer.BlockCopy(privateKey, 0, ret, _sessionIDLength, privateKey.Length);

            return ret;
        }

        /*
         * https://crypto.stackexchange.com/questions/16196/what-is-a-generator
         * 
         * https://tools.ietf.org/html/rfc2631
         * 
         * https://tools.ietf.org/html/rfc8268
         * 
         * https://tools.ietf.org/html/rfc5114 
         */

        public void DispatchKeyExchange()
        {
            
        }
    }
}
