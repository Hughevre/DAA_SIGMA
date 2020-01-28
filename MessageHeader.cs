using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BUS_DAA_SIGMA
{
    static class MessageHeader
    {
        public enum BasicType
        {
            TCPHANDSHAKE,
            POST,
            SIGMA
        }

        public enum SigmaType
        {
            PHello,
            QResponse,
            PAffirmation
        }
    }
}
