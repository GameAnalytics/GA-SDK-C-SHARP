using System;
using System.Collections.Generic;
#if WINDOWS_WSA || WINDOWS_UWP
using Windows.System.Threading;
using System.Threading.Tasks;
#else
using System.Threading;
#endif
using GameAnalyticsSDK.Net.Logging;

namespace GameAnalyticsSDK.Net.Threading
{
	public class GAThreading
	{
        private static bool shouldThreadrun = false;
        private static readonly GAThreading _instance = new GAThreading ();
		private const int ThreadWaitTimeInMs = 1000;
		private readonly PriorityQueue<long, TimedBlock> blocks = new PriorityQueue<long, TimedBlock>();
		private readonly object threadLock = new object();


		private GAThreading()
		{
			GALogger.D("Initializing GA thread...");

            StartThread();
		}

		private static GAThreading Instance
		{
			get 
			{
				return _instance;
			}
		}

		public static void Run()
		{
			GALogger.D("Starting GA thread");

			try
			{
				while(shouldThreadrun)
				{
					TimedBlock timedBlock;

					while((timedBlock = GetNextBlock()) != null)
					{
                        timedBlock.block();
                    }


#if WINDOWS_WSA || WINDOWS_UWP
                    Task.Delay(1000).Wait();
#else
                    Thread.Sleep(ThreadWaitTimeInMs);
#endif
                }
            }
			catch(Exception e)
			{
				GALogger.E("Error on GA thread");
				GALogger.E(e.ToString());
			}

			GALogger.D("Ending GA thread");
		}

        public static void PerformTaskOnGAThread(string blockName, Action taskBlock)
		{
			PerformTaskOnGAThread(blockName, taskBlock, 0);
		}

		public static void PerformTaskOnGAThread(string blockName, Action taskBlock, long delayInSeconds)
		{
			lock(Instance.threadLock)
			{
				DateTime time = DateTime.Now;
				time = time.AddSeconds(delayInSeconds);

				TimedBlock timedBlock = new TimedBlock(time, taskBlock, blockName);
				Instance.AddTimedBlock(timedBlock);
			}
		}

		public static void ScheduleTimer(double interval, string blockName, Action callback)
		{
			lock(Instance.threadLock)
			{
				DateTime time = DateTime.Now;
				time = time.AddSeconds(interval);

				TimedBlock timedBlock = new TimedBlock(time, callback, blockName);
				Instance.AddTimedBlock(timedBlock);
			}
		}

		private void AddTimedBlock(TimedBlock timedBlock)
		{
			this.blocks.Enqueue(timedBlock.deadline.Ticks, timedBlock);
		}

		private static TimedBlock GetNextBlock()
		{
			lock(Instance.threadLock)
			{
				DateTime now = DateTime.Now;

				if(Instance.blocks.HasItems && Instance.blocks.Peek().deadline.CompareTo(now) <= 0)
				{
					return Instance.blocks.Dequeue();
				}

				return null;
			}
		}

#if WINDOWS_WSA || WINDOWS_UWP
        public async static void StartThread()
#else
        public static void StartThread()
#endif
        {
            GALogger.D("StartThread called");
            if (!shouldThreadrun)
			{
				shouldThreadrun = true;
#if WINDOWS_WSA || WINDOWS_UWP
            	await ThreadPool.RunAsync(o => Run());
#else
				Thread thread = new Thread(new ThreadStart(Run));
				thread.Priority = ThreadPriority.Lowest;
				thread.Start();
#endif
			}
        }

		public static void StopThread()
		{
            GALogger.D("StopThread called");
            shouldThreadrun = false;
		}
    }
}

