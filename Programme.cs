using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerTest server = new ServerTest();
            server.Start();
            
            //Console.ReadKey();
        }
    }
}
