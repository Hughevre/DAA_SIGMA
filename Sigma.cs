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
        private readonly Random _random;
        private readonly int    _bigIntBufferLength;

        private BigInteger      _s;
        private BigInteger      _g;
        private BigInteger      _x;

        private Sender          _connection;

        public Sigma(Sender connection)
        {
            _random             = new Random();
            _bigIntBufferLength = 256;
            _connection         = connection;
            
        }

        public void BeginKeyExchange()
        {
            _s = GetRandomPositiveBigInteger();
            _g = new BigInteger();
            _x = GetRandomPositiveBigInteger();
        }

        private BigInteger GetRandomPositiveBigInteger()
        {
            byte[] buffer = new byte[_bigIntBufferLength];
            _random.NextBytes(buffer);
            buffer[_bigIntBufferLength - 1] &= 0x7F; // Force sign bit to positive.  See https://docs.microsoft.com/en-us/dotnet/api/system.numerics.biginteger.-ctor.
            return new BigInteger(buffer);
        }
    }
}
