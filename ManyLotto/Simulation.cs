using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManyLotto
{
    class Simulation : Control
    {
        private long[] computerNumbers = new long[MAX_PICK_NUMS];
        private int computerPB = 0;

        // Base
        public void StartSimulation()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "log.txt");
            string filePathTimes = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "times.txt");
            var numbersConcurrentBag = new ConcurrentBag<long[]>();
            var stopWatch = new Stopwatch();

            // Create log.txt -- If it already exists empty it
            if (!File.Exists(filePath))
                File.CreateText(filePath);
            else
                File.Create(filePath).Close();

            stopWatch.Start();

            // Create winning numbers
            CreateAndShuffleWinningNumbers();

            // Create bag to fill with user numbers
            FillBag(numbersConcurrentBag);

            // Write to log
            WriteToLog(numbersConcurrentBag, filePath);
            stopWatch.Stop();

            // Display time elapsed
            TimeElapsed(filePathTimes, stopWatch);
        }

        // Create and shuffle winning numbers
        private void CreateAndShuffleWinningNumbers()
        {
            // Create 59 numbers
            var tempComputerNumbers = Enumerable.Range(1, 59).ToArray();

            // Shuffle them -- Fisher-Yates
            for (int i = tempComputerNumbers.Length; i > 0; i--)
            {
                int j = RandomHelper.Instance.Next(i);

                var temp = tempComputerNumbers[j];
                tempComputerNumbers[j] = tempComputerNumbers[i - 1];
                tempComputerNumbers[i - 1] = temp;
            }
            // Fill computerNumbers
            for (int i = 0; i < MAX_PICK_NUMS; i++)
            {
                computerNumbers[i] = tempComputerNumbers[i];
            }
            // PowerBall number
            computerPB = RandomHelper.Instance.Next(1, 36);

            // Faster comparison (sorted | unsorted) vs (unsorted | unsorted)
            Array.Sort(tempComputerNumbers);
        }

        // Create and shuffle user numbers
        private long[] CreateAndShuffleUserNumbers()
        {
            var userNumbers = new long[MAX_PICK_NUMS];

            // Have to do this because Enumerable.Range().ToArray() is int[] -- we need a long[]
            var tempUserNumbers = new long[59];
            for (int i = 0; i < tempUserNumbers.Length; i++)
            {
                tempUserNumbers[i] = i + 1;
            }

            for (int i = tempUserNumbers.Length; i > 0 ; i--)
            {
                int j = RandomHelper.Instance.Next(i);

                var temp = tempUserNumbers[j];
                tempUserNumbers[j] = tempUserNumbers[i - 1];
                tempUserNumbers[i - 1] = temp;
            }

            for (int i = 0; i < MAX_PICK_NUMS; i++)
            {
                userNumbers[i] = tempUserNumbers[i];
            }
            userPowerBall();

            // Might remove this later, just seeing how this will look in log
            Array.Sort(userNumbers);

            return userNumbers;
        }

        // Generate new PowerBall
        private int userPowerBall()
        {
            int userPB = RandomHelper.Instance.Next(1, 36);

            return userPB;
        }

        // Write to log.txt
        private void WriteToLog(ConcurrentBag<long[]> numbersConcurrentBag, string filePath)
        {
            using (var streamWriter = new StreamWriter(filePath))
            {
                streamWriter.Write("Winning numbers: ");
                foreach (var element in computerNumbers)
                {
                    streamWriter.Write("{0}, ", element);
                }
                streamWriter.Write(" - [{0}]\n", computerPB);

                foreach (var element in numbersConcurrentBag)
                {
                    streamWriter.Write("[");
                    foreach (var index in element)
                    {
                        streamWriter.Write("{0}, ", index);
                    }
                    streamWriter.WriteLine("] - [{0}] - Thread: {1}", userPowerBall(), Thread.CurrentThread.ManagedThreadId);
                }
            }
        }

        // Display stopwath information
        private void TimeElapsed(string filePathTimes, Stopwatch stopWatch)
        {
            using (var fileWriter = new StreamWriter(filePathTimes, true))
            {
                fileWriter.WriteLine("{0} simulations processed in {1}", userChoice, stopWatch.Elapsed);
            }
            Console.WriteLine("Time elapsed: {0}", stopWatch.Elapsed);
        }

        // Fill bag with generated numbers
        private void FillBag(ConcurrentBag<long[]> numbersConcurrentBag)
        {
            Parallel.For(0, userChoice, i => numbersConcurrentBag.Add(CreateAndShuffleUserNumbers()));
        }
    }
}
