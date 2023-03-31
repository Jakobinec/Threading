using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Threading.StudentLine
{
    internal class StudentLineLocker
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
            private object locker = new object();

            public Student(int number) : base(number)
            {
            }

            public override void SayNumber()
            {
                //Console.WriteLine("Используется поток номер: " + Thread.CurrentThread.GetHashCode());

                if (_number - _currentLineNumber == 1)
                {
                    Console.WriteLine(_number);
                    _currentLineNumber = _number;
                    return;
                }

                lock (locker) 
                {
                    if (_number - _currentLineNumber == 1)
                    {
                        Console.WriteLine(_number);
                        _currentLineNumber = _number;
                        return;
                    }



                    //Console.WriteLine("Внутри локкера поток номер: " + Thread.CurrentThread.GetHashCode());
                    //if (_number - _currentLineNumber == 1)
                    //{
                    //    Console.WriteLine(_number);
                    //    _currentLineNumber = _number;
                    //    return;
                    //}
                    //else 
                    //{
                    //    SayNumber();
                    //}
                }

                //SayNumber();

            }
        }
    }
}
