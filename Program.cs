using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS_DAA_SIGMA
{
    class Program
    {
        static void Main(string[] args)
        {
            do
            {
                UI.DrawUI();
                var input = Console.ReadLine();
                Commands.Decode(input);
            } while (true);
        }
    }
}
