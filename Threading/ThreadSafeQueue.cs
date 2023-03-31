using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Threading
{
    internal class ThreadSafeQueue
    {
        private static readonly Random rnd = new Random();

        public static void RunExample()
        {
            Console.WriteLine($"start");
            var queue = new CustomQueue<int>();
            var p0 = RunProviderTask("prov_1", queue);
            var c0 = RunConsumerTask("cons_1", queue);
            var c1 = RunConsumerTask("cons_2", queue);
            Console.ReadLine();
        }

        private static Task RunProviderTask(string threadName, Queue<int> queue)
        {
            return Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.Name = threadName;
                while (true)
                {
                    var item = rnd.Next(ushort.MaxValue);
                    queue.Enqueue(item);
                    Console.WriteLine($"{ShowThreadName()} added {item} (count {queue.Count})");
                    Thread.Sleep(50);
                }
            });
        }

        private static Task RunConsumerTask(string threadName, Queue<int> queue)
        {
            return Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.Name = threadName;
                while (true)
                {
                    try
                    {
                        var items = new List<int>();
                        while (queue.Count > 0)
                        {
                            items.Add(queue.Dequeue());
                        }
                        if (items.Count > 0)
                        {
                            Console.WriteLine($"{ShowThreadName()} poped {string.Join(", ", items)}");
                        }
                        Thread.Sleep(400);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            });
        }

        private static string ShowThreadName()
        {
            return $"<thr-{Thread.CurrentThread.Name}>";
        }        
    }


    class CustomQueue<T> : Queue<T>
    {
        private Queue<T> queue = new Queue<T>();

        public void Enqueue(T item) 
        {
            lock (queue) 
            {
                queue.Enqueue(item);
            }
        }

        public T Dequeue() 
        {
            lock (queue) 
            {
               return queue.Dequeue();
            }
        }

        
         
    }
}

