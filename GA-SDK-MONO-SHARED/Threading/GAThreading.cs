using System;
using System.Collections.Generic;
#if UNITY_WSA || WINDOWS_UWP
using Windows.System.Threading;
using System.Threading.Tasks;
#elif !UNITY_WEBGL
using System.Threading;
#else
using System.Collections;
using UnityEngine;
#endif
using GameAnalyticsSDK.Net.Logging;

namespace GameAnalyticsSDK.Net.Threading
{
	public class GAThreading
	{
		private static readonly GAThreading _instance = new GAThreading ();
		#if !UNITY_WEBGL
		private const int ThreadWaitTimeInMs = 1000;
		#else
		private const int ThreadWaitTimeInSec = 1;
		#endif
		private readonly PriorityQueue<long, TimedBlock> blocks = new PriorityQueue<long, TimedBlock>();
		private readonly Dictionary<long, TimedBlock> id2TimedBlockMap = new Dictionary<long, TimedBlock>();
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

#if !UNITY_WEBGL
		public static void Run()
		{
			GALogger.D("Starting GA thread");

			try
			{
				while(true)
				{
					TimedBlock timedBlock;

					while((timedBlock = GetNextBlock()) != null)
					{
						if(!timedBlock.ignore)
						{
							timedBlock.block();
						}
					}


#if UNITY_WSA || WINDOWS_UWP
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
#else
		public static IEnumerator Run()
		{
			GALogger.D("Starting GA thread");

			while(true)
			{
				try
				{
					TimedBlock timedBlock;

					while((timedBlock = GetNextBlock()) != null)
					{
						if(!timedBlock.ignore)
						{
							timedBlock.block();
						}
					}
				}
				catch(Exception e)
				{
					GALogger.E("Error on GA thread");
					GALogger.E(e.ToString());
					break;
				}

				yield return new WaitForSeconds(ThreadWaitTimeInSec);
			}

			GALogger.D("Ending GA thread");
		}
#endif

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
				Instance.id2TimedBlockMap.Add(timedBlock.id, timedBlock);
				Instance.AddTimedBlock(timedBlock);
			}
		}

		public static long ScheduleTimer(double interval, string blockName, Action callback)
		{
			lock(Instance.threadLock)
			{
				DateTime time = DateTime.Now;
				time = time.AddSeconds(interval);

				TimedBlock timedBlock = new TimedBlock(time, callback, blockName);
				Instance.id2TimedBlockMap.Add(timedBlock.id, timedBlock);
				Instance.AddTimedBlock(timedBlock);
				return timedBlock.id;
			}
		}

		public static void IgnoreTimer(long blockIdentifier)
		{
			lock(Instance.threadLock)
			{
				TimedBlock timedBlock;

				if(Instance.id2TimedBlockMap.TryGetValue(blockIdentifier, out timedBlock))
				{
					timedBlock.ignore = true;
				}
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

#if UNITY_WSA || WINDOWS_UWP
        private async static void StartThread()
#else
        private static void StartThread()
#endif
        {
#if UNITY_WSA || WINDOWS_UWP
            await ThreadPool.RunAsync(o => Run());
#elif !UNITY_WEBGL
            Thread thread = new Thread(new ThreadStart(Run));
			thread.Priority = ThreadPriority.Lowest;
			thread.Start ();
#endif
        }
    }
}

