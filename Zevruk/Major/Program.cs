using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zevruk
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            WinApi.TimeBeginPeriod(1);
            while (!Major.Instance.Closed)
            {
                Major.Instance.Work();
            }

            WinApi.TimeEndPeriod(1);
        }
    }
}
