using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Threading.StudentLine;

namespace Threading
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //SpinWaitSynchronization.RunExample(); // +
            //BarrierSynchronization.RunExample(); // +
            EventSynchronization.RunExample(); // + (но медленно)

            //ThreadSafeQueue.RunExample();
            //DnsQueries.RunExample();
        }

    }

}
