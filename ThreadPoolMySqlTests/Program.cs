using MySqlConnector;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPoolMySqlTests
{
    class Program
    {
        static string connString = "Uid=root;Password=root;Server=localhost;Port=3307;SslMode=None;convert zero datetime=True;";
        static void Main(string[] args)
        {
            Console.WriteLine("Press 1 to run 'TestThreads', press 2 to run 'TestTask'");
            string line = Console.ReadLine();
            line = line.Trim();

            if (line.Equals("1"))
                TestThreads();
            else if (line.Equals("2"))
                TestTask();
            else
                Console.WriteLine("Not a valid input.");
            Console.ReadLine();
        }

        /// <summary>
        /// This method works, no timeouts. About 12-14 seconds for 200 connections
        /// </summary>
        static void TestThreads()
        {
            int counter = 0;
            int doneCounter = 0;
            int nbrIterations = 200;
            Stopwatch sW = new Stopwatch();
            sW.Start();
            for (int i = 0; i < nbrIterations; i++)
            {
                new Thread(() =>
                {
                    int nbr = Interlocked.Increment(ref counter);
                    MySqlConnection conn = new MySqlConnection(connString);
                    conn.Open();
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}\t{Thread.CurrentThread.ManagedThreadId}\tCreated connection {nbr}!");
                    Thread.Sleep(5000);
                    conn.Close();
                    Interlocked.Increment(ref doneCounter);
                }).Start();
            }
            while(doneCounter != nbrIterations)
            {
                Thread.Sleep(1);                   
            }
            sW.Stop();
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}\t{Thread.CurrentThread.ManagedThreadId}\tExecution took {sW.ElapsedMilliseconds}!");
        }

        /// <summary>
        /// This is much slower than using TestThreads, in the order of 200 seconds or so. Why is using ThreadPool so much slower?
        /// </summary>
        static void TestTask()
        {
            int counter = 0;
            int doneCounter = 0;
            int nbrIterations = 200;
            Stopwatch sW = Stopwatch.StartNew();
            for (int i = 0; i < nbrIterations; i++)
            {
                Task.Run(async () =>
                {
                    int j = Interlocked.Increment(ref counter);
                    MySqlConnection conn = new MySqlConnection(connString);
                    await conn.OpenAsync();
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}\t{Thread.CurrentThread.ManagedThreadId}\tCreated connection {j}!");
                    await Task.Delay(5000);
                    await conn.CloseAsync();
                    Interlocked.Increment(ref doneCounter);
                });
            }
            while (doneCounter != nbrIterations)
            {
                Thread.Sleep(1);
            }
            sW.Stop();
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}\t{Thread.CurrentThread.ManagedThreadId}\tExecution took {sW.ElapsedMilliseconds}!");
        }
    }
}
