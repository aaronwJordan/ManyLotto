using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ManyLotto
{
    class Simulation : Control
    {
        private long[] computerNumbers = new long[MAX_PICK_NUMS];
        private int computerPowerBall;
        
        // Base
        public void StartSimulation()
        {
            var filePathLog = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "log.txt");
            var filePathTimes = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "times.txt");
            var filePathFiveHit = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "fivehit.txt");
            var numbersConcurrentBag = new ConcurrentBag<long[]>();
            var powerBallConcurrentBag = new ConcurrentBag<long>();
            var stopWatch = new Stopwatch();

            // Create log.txt -- If it already exists empty it
            if (!File.Exists(filePathLog))
                File.CreateText(filePathLog);
            else
                File.Create(filePathLog).Close();

            stopWatch.Start();

            // Create winning numbers
            CreateAndShuffleWinningNumbers();

            // Create bag to fill with user numbers
            FillBagNumbers(numbersConcurrentBag);

            // Create bag to fill with PowerBall numbers
            FillBagPowerBall(powerBallConcurrentBag);

            // Write to log
            WriteToLog(numbersConcurrentBag, powerBallConcurrentBag, filePathLog, filePathFiveHit);
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
            computerPowerBall = RandomHelper.Instance.Next(1, 36);

            // Faster comparison (sorted | unsorted) vs (unsorted | unsorted)
            Array.Sort(tempComputerNumbers);
        }

        // Fill bag with generated numbers
        private void FillBagNumbers(ConcurrentBag<long[]> numbersConcurrentBag)
        {
            Parallel.For(0, userChoice, i => numbersConcurrentBag.Add(CreateAndShuffleUserNumbers()));
        }

        private void FillBagPowerBall(ConcurrentBag<long> powerBallConcurrentBag)
        {
            Parallel.For(0, userChoice, i => powerBallConcurrentBag.Add(userPowerBall()));
        }

        // Compare winning numbers to user numbers
        private int NumberCompare(long[][] tempJaggedNumbersArray, long powerBallCounter)
        {
            var winHit = 0;

            for (var i = 0; i < MAX_PICK_NUMS; i++)
            {
                for (var j = 0; j < MAX_PICK_NUMS; j++)
                {
                    if (tempJaggedNumbersArray[powerBallCounter][j] == computerNumbers[i])
                        winHit++;
                }
            }

            return winHit;
        }

        // Match number of hits
        private void MatchHits(int tempNumberCompare, ref long threeHit, ref long fourHit, ref long fiveHit)
        {
            switch (tempNumberCompare)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    threeHit++;
                    break;
                case 4:
                    fourHit++;
                    break;
                case 5:
                    fiveHit++;
                    Console.WriteLine("FIVE HIT");
                    break;
                default:
                    Console.WriteLine("Default at MatchHits()");
                    break;
            }
        }

        // Print special five hit to fivehitlog.txt
        private void LogFiveHit(StreamWriter streamWriterFiveHit, long powerBallCounter)
        {
            streamWriterFiveHit.WriteLine(powerBallCounter);
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

            // Sort array numbers
            Array.Sort(userNumbers);

            return userNumbers;
        }

        // Write to log.txt
        private void WriteToLog(ConcurrentBag<long[]> numbersConcurrentBag, ConcurrentBag<long> powerBallConcurrentBag, string filePathLog, string filePathFiveHit)
        {
            long threeHit = 0, fourHit = 0, fiveHit = 0, powerBallCounter = 0;
            var tempPowerBallArray = powerBallConcurrentBag.ToArray();
            var tempJaggedNumbersArray = numbersConcurrentBag.ToArray();

            using (var streamWriter = new StreamWriter(filePathLog))
            {
                streamWriter.Write("Winning numbers: ");
                foreach (var element in computerNumbers)
                {
                    streamWriter.Write("{0}, ", element);
                }
                streamWriter.Write("- [{0}]", computerPowerBall);

                using (var streamWriterFiveHit = new StreamWriter(filePathFiveHit))
                {
                    foreach (var element in numbersConcurrentBag)
                    {
                        var tempNumberCompare = NumberCompare(tempJaggedNumbersArray, powerBallCounter);
                        var tempThreadPrintCounter = 0;

                        if (tempNumberCompare > 0)
                        {
                            streamWriter.Write("\nPlay: {0} - {1} hit(s) - {2} PowerBall - [", powerBallCounter + 1, tempNumberCompare, ComparePowerBall(powerBallCounter, tempPowerBallArray));

                            // Accumulate hits
                            MatchHits(tempNumberCompare, ref threeHit, ref fourHit, ref fiveHit);

                            // Log each five hit
                            LogFiveHit(streamWriterFiveHit, powerBallCounter);

                            foreach (var index in element)
                            {
                                // Print out every number with a comma and space except the last element
                                streamWriter.Write(tempThreadPrintCounter < 4 ? "{0}, " : "{0}", index);

                                if (tempThreadPrintCounter == (MAX_PICK_NUMS - 1))
                                    streamWriter.Write("] - [{0}] - Thread: {1}", tempPowerBallArray[powerBallCounter], Thread.CurrentThread.ManagedThreadId);
                                tempThreadPrintCounter++;
                            }
                        }
                        powerBallCounter++;
                    }
                }
            }
            Console.WriteLine("\nHits: ({0}) - 3 hits | ({1}) - 4 hits | ({2}) - 5 hits\n", threeHit, fourHit, fiveHit);
        }

        // Compare winning PowerBall to user generated ones
        private bool ComparePowerBall(long powerBallCounter, long[] tempPowerBallArray)
        {
            if (computerPowerBall == tempPowerBallArray[powerBallCounter])
                return true;
            return false;
        }

        // Generate new PowerBall
        private int userPowerBall()
        {
            int userPowerBall = RandomHelper.Instance.Next(1, 36);

            return userPowerBall;
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
    }
}
