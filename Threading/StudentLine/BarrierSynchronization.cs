using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Threading.StudentLine
{
    internal class BarrierSynchronization
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
                        Console.WriteLine("Создан поток номер: " + Thread.CurrentThread.GetHashCode());
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
            private static Barrier _barrier = new Barrier(20);
            private static int _currentLineNumber = 0;

            public Student(int number) : base(number)
            {
            }

            public override void SayNumber()
            {
                Console.WriteLine("Используется поток номер: " + Thread.CurrentThread.GetHashCode());

                if (_number - _currentLineNumber == 1)
                {
                    Console.WriteLine("Называет номер: " + Thread.CurrentThread.GetHashCode());
                    Console.WriteLine(_number);
                    _currentLineNumber = _number;
                    _barrier.RemoveParticipant();
                    return;
                }

                Console.WriteLine("Дошёл до барьера: " + Thread.CurrentThread.GetHashCode());
                _barrier.SignalAndWait();

                SayNumber();

            }
        }
    }
}
