using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Threading.StudentLine
{
    internal class EventSynchronization
    {
        public static void RunExample() 
        {
            //ThreadPool.GetMaxThreads(out int threadCount, out int test);
            //Console.WriteLine($"Сейчас потоков: {threadCount}");
            //Console.WriteLine($"Сейчас потоков: {test}");
            var line = GetStudentLine<Student>(20);
            Task.WaitAll(line.Select(s => Task.Run(() => 
            {
                
                //Console.WriteLine("Создан поток номер: " + Thread.CurrentThread.GetHashCode());
                s.SayNumber();
            })).ToArray());

            Console.ReadLine();
        }

        private static T[] GetStudentLine<T>(int count)
            where T : AbstractStudent
        {
            var line = new T[count];
            var rnd = new Random();
            for (int i = 0; i < line.Length; i++)
            {
                var getNextRndIndex = new Func<int>(() =>
                {
                    int rndIndex;
                    do
                    {
                        rndIndex = rnd.Next(line.Length);
                    } while (line[rndIndex] != null);
                    return rndIndex;
                });

                line[getNextRndIndex()] = (T)Activator.CreateInstance(typeof(T), new object[] { i + 1 });
            }
            return line;
        }

        public abstract class AbstractStudent
        {
            protected readonly int _number;

            public AbstractStudent(int number)
            {
                _number = number;
            }

            public abstract void SayNumber();
        }

        public sealed class Student : AbstractStudent
        {
            private static int _currentLineNumber = 0;
            private static ManualResetEvent _eventHandler = new ManualResetEvent(true);
            public Student(int number) : base(number)
            {
            }

            public override void SayNumber()
            {
                while (_number - _currentLineNumber != 1) 
                {
                    _eventHandler.WaitOne();
                }
                
                Console.WriteLine(_number);
                _currentLineNumber = _number;
                _eventHandler.Set();
            }
        }
    }
}
