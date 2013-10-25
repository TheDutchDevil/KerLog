using ClientListener.KerLogListener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientListener
{
    class Program
    {
        static void Main(string[] args)
        {
            Listener listener = new Listener(10000);
            listener.Start();
            Console.ReadLine();
            listener.Stop();
        }
    }
}
