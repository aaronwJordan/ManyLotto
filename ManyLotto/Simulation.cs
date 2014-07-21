using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ManyLotto
{
	class Simulation : Control
	{
		private static readonly object LockMethod = new object();
		private long[] computerNumbers = new long[MAX_PICK_NUMS];
		private int computerPowerBall;

		/*  
		 *  So, FillBagNumbers() which uses a concurrent bag fully fills up before comparisons ever begin.
		 *  Thus, A huge bag is created which takes a metric shit-ton of memory.
		 *  
		 *  The idea now is to somehow split up FillBagNumbers()
		 *  
		 *  Fill bag up (25% of 100)? -> Compare -> Empty -> Fill (25% -> 50% of 100) etc -> (100% of 100) -> Done
		 *  
		 */

		// Base
		public void StartSimulation()
		{
			var filePathLog = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "log.txt");
			var filePathTimes = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "times.txt");
			var filePathFiveHit = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "fivehit.txt");
			var numbersConcurrentBag = new ConcurrentBag<long[]>();
			var powerBallConcurrentBag = new ConcurrentBag<long>();
			var stopWatch = new Stopwatch();
			long temp25 = 0, temp50 = 0, temp75 = 0;
			const int numberOfChunks = 4;
			var chunkCounter = 1;

			// Create log.txt -- If it already exists empty it
			if (!File.Exists(filePathLog))
				File.CreateText(filePathLog);
			else
				File.Create(filePathLog).Close();

			// Create fivehit.txt -- If it already exists empty it
			if (!File.Exists(filePathFiveHit))
				File.CreateText(filePathFiveHit);
			else
				File.Create(filePathFiveHit).Close();

			stopWatch.Start();

			// Create winning numbers
			CreateAndShuffleWinningNumbers();

			// Eventually do percentages here?
			ChunkUserShoice(numberOfChunks, ref temp25, ref temp50, ref temp75);

			// Create bag to fill with PowerBall numbers
			FillBagPowerBall(powerBallConcurrentBag);

			// Write to log
			WriteToLog(numbersConcurrentBag, powerBallConcurrentBag, filePathLog, filePathFiveHit, ref chunkCounter, temp25, temp50, temp75);
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

		// Chunk userChoice up in pieces to calculate separately
		private void ChunkUserShoice(int numberOfChunks, ref long temp25, ref long temp50, ref long temp75)
		{
			// Chunking one large userChoice number into (4) separate chunks for memory concerns (IO swapping pls)
			// 100% -> (0-25%/26-50%/51-75%/76-100%)
			temp25 = (userChoice / numberOfChunks);
			temp50 = (temp25 * (numberOfChunks - 2));
			temp75 = (temp25 + temp50);
		}

		// Fill bag with generated numbers
		private void FillBagNumbers(ConcurrentBag<long[]> numbersConcurrentBag, int chunkCounter, long temp25, long temp50, long temp75)
        {
			//TODO: SPREAD OUT TO SEPARATE METHODS, THIS IS REDICULOUS. ALSO WRITETOLOG()
			//TODO: EMPTY THE JAGGED ARRAY OUT IN WRITETOLOG()
			//TODO: FIX LOGFIVEHIT()

			bool casea = false;
			bool caseb = false;
			bool casec = false;

			bool case2a = false;
			bool case2b = false;
			bool case2c = false;

			bool case3a = false;
			bool case3b = false;
			bool case3c = false;

			bool case4a = false;
			bool case4b = false;
			bool case4c = false;

			switch (chunkCounter)
			{
				case 1:
					Console.WriteLine("\nCase 1 fired");
					Parallel.For(0, temp25, i => 
					{
						lock (LockMethod)
						{
							numbersConcurrentBag.Add(CreateAndShuffleUserNumbers());

							if (!casea)
							{
								if (numbersConcurrentBag.Count() > (Math.Floor(temp25 * .25))) // a
								{
									Console.WriteLine("25%..."); 
									casea = true;
								}	
							}
								
							if (!caseb)
							{
								if (numbersConcurrentBag.Count() > (Math.Floor(temp25 * .5))) // b
								{
									Console.WriteLine("50%...");
									caseb = true;
								}
							}
								
							if (!casec)
							{
								if (numbersConcurrentBag.Count() > (Math.Floor(temp25 * .75))) // c
								{
									Console.WriteLine("75%...");
									casec = true;
								}	
							}
						}
					});
					break;
				case 2:
					Console.WriteLine("\n\nCase 2 fired");
					Parallel.For(temp25, temp50, i =>
					{
						lock (LockMethod)
						{
							numbersConcurrentBag.Add(CreateAndShuffleUserNumbers());

							if (!case2a)
							{
								if (numbersConcurrentBag.Count() > (Math.Floor(temp25 * .25))) // a
								{
									Console.WriteLine("25%...");
									case2a = true;
								}
							}

							if (!case2b)
							{
								if (numbersConcurrentBag.Count() > (Math.Floor(temp25 * .5))) // b
								{
									Console.WriteLine("50%...");
									case2b = true;
								}
							}

							if (!case2c)
							{
								if (numbersConcurrentBag.Count() > (Math.Floor(temp25 * .75))) // c
								{
									Console.WriteLine("75%...");
									case2c = true;
								}
							}
						}
					});
					break;
				case 3:
					Console.WriteLine("\n\nCase 3 fired");
					Parallel.For(temp50, temp75, i => 
					{
						lock (LockMethod)
						{
							numbersConcurrentBag.Add(CreateAndShuffleUserNumbers());

							if (!case3a)
							{
								if (numbersConcurrentBag.Count() > (Math.Floor(temp25 * .25))) // a
								{
									Console.WriteLine("25%...");
									case3a = true;
								}
							}

							if (!case3b)
							{
								if (numbersConcurrentBag.Count() > (Math.Floor(temp25 * .5))) // b
								{
									Console.WriteLine("50%...");
									case3b = true;
								}
							}

							if (!case3c)
							{
								if (numbersConcurrentBag.Count() > (Math.Floor(temp25 * .75))) // c
								{
									Console.WriteLine("75%...");
									case3c = true;
								}
							}
						}
					});
					break;
				case 4:
					Console.WriteLine("\n\nCase 4 fired");
					Parallel.For(temp75, userChoice, i => 
					{
						lock (LockMethod)
						{
							numbersConcurrentBag.Add(CreateAndShuffleUserNumbers());

							if (!case4a)
							{
								if (numbersConcurrentBag.Count() > (Math.Floor(temp25 * .25))) // a
								{
									Console.WriteLine("25%...");
									case4a = true;
								}
							}

							if (!case4b)
							{
								if (numbersConcurrentBag.Count() > (Math.Floor(temp25 * .5))) // b
								{
									Console.WriteLine("50%...");
									case4b = true;
								}
							}

							if (!case4c)
							{
								if (numbersConcurrentBag.Count() > (Math.Floor(temp25 * .75))) // c
								{
									Console.WriteLine("75%...");
									case4c = true;
								}
							}
						}
					});
					break;
				default:
					Console.WriteLine("DEFAULTED AT CHUNKCOUNTER SWITCH FILLBAGNUMBERS()");
					break;
			}
        }

		// Fill bag with PowerBalls
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

			for (int i = tempUserNumbers.Length; i > 0; i--)
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
		private void WriteToLog(ConcurrentBag<long[]> numbersConcurrentBag, ConcurrentBag<long> powerBallConcurrentBag, string filePathLog, string filePathFiveHit, ref int chunkCounter, long temp25, long temp50, long temp75)
		{
			long threeHit = 0, fourHit = 0, fiveHit = 0, powerBallCounter = 0;
			long[] tempPowerBallArray = new long[userChoice];
			long lineCount = 0;

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
					for (var i = 0; i < 4; i++)
					{
						powerBallCounter = 0;
						long[] takeOut;
						while (!numbersConcurrentBag.IsEmpty)
							numbersConcurrentBag.TryTake(out takeOut);

						switch (i)
						{
							case 0:
								FillBagNumbers(numbersConcurrentBag, chunkCounter, temp25, temp50, temp75);
								break;
							case 1:
								FillBagNumbers(numbersConcurrentBag, chunkCounter, temp25, temp50, temp75);
								break;
							case 2:
								FillBagNumbers(numbersConcurrentBag, chunkCounter, temp25, temp50, temp75);
								break;
							case 3:
								FillBagNumbers(numbersConcurrentBag, chunkCounter, temp25, temp50, temp75);
								break;
							default:
								Console.WriteLine("DEFAULTED AT CHUNKCOUNTER SWITCH WRITETOLOG()");
								break;
						}

						var tempJaggedNumbersArray = numbersConcurrentBag.ToArray();

						// We only need to .ToArray() the powerball array once
						if (i == 0)
							tempPowerBallArray = powerBallConcurrentBag.ToArray();

						foreach (var element in numbersConcurrentBag)
						{
							var tempNumberCompare = NumberCompare(tempJaggedNumbersArray, powerBallCounter);
							var tempThreadPrintCounter = 0;

							// We really don't want to print any sets that didn't match even 1 winning number
							if (tempNumberCompare > 0)
							{
								streamWriter.Write("\nPlay: {0} - {1} hit(s) - {2} PowerBall - [", lineCount + 1, tempNumberCompare, ComparePowerBall(powerBallCounter, tempPowerBallArray));

								// Accumulate hits
								MatchHits(tempNumberCompare, ref threeHit, ref fourHit, ref fiveHit);

								// Log each five hit
								// TODO: IF (MATCHITS > 5) -- NOT DONE
								if (fiveHit > 0)
								{
									LogFiveHit(streamWriterFiveHit, lineCount);
								}

								foreach (long index in element)
								{
									// Print out every number with a comma and space except the last element
									streamWriter.Write(tempThreadPrintCounter < 4 ? "{0}, " : "{0}", index);

									if (tempThreadPrintCounter == (MAX_PICK_NUMS - 1))
										streamWriter.Write("] - [{0}]", tempPowerBallArray[powerBallCounter]);
									tempThreadPrintCounter++;
								}
								lineCount++;
							}
							powerBallCounter++;
						}
						chunkCounter++;
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
			// http://xkcd.com/221/
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
