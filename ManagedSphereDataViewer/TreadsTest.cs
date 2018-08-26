using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace ManagedSphereDataViewer
{
    static class TreadsTest
    {
        private static int iterations = 1000;

        public static void Test ()
        {
            Stopwatch mywatch = new Stopwatch();

            Console.WriteLine("Plinq Execution");

            mywatch.Start();
            ProcessWithPLinq();
            mywatch.Stop();

            Console.WriteLine("Time consumed by ProcessWithPLinq is : " + mywatch.ElapsedTicks.ToString());
            mywatch.Reset();

            Console.WriteLine("Thread Pool Execution");

            mywatch.Start();
            ProcessWithThreadPoolMethod();
            mywatch.Stop();

            Console.WriteLine("Time consumed by ProcessWithThreadPoolMethod is : " + mywatch.ElapsedTicks.ToString());
            mywatch.Reset();


            Console.WriteLine("Thread run Execution");

            mywatch.Start();
            ProcessWithRun();
            mywatch.Stop();

            Console.WriteLine("Time consumed by ProcessWithRun is : " + mywatch.ElapsedTicks.ToString());
            //Console.ReadKey();
        }

        static void ProcessWithThreadPoolMethod()
        {
            for (int i = 0; i <= iterations; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(Process));
            }
        }

        static void ProcessWithThreadMethod()
        {
            for (int i = 0; i <= iterations; i++)
            {
                Thread obj = new Thread(Process);
                obj.Start();
            }
        }

        static void ProcessWithPLinq()
        {
            Enumerable.Range(1, iterations).AsParallel().ForAll((x) =>
                {
                    Process(null);
                });
        }

        static void ProcessWithRun()
        {
            Task[] tasks = Enumerable.Range(0, iterations).Select(i =>
                // Create task here.
                Task.Run(() => {
                    // Do work.
                })
                ).ToArray();

                        // Wait on all the tasks.
                Task.WaitAll(tasks);
            }

        static void Process(object callback)
        {

        }
    }
}
