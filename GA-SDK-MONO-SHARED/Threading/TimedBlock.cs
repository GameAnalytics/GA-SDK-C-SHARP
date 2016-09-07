using System;

namespace GameAnalyticsSDK.Net
{
	internal class TimedBlock : IComparable<TimedBlock>
	{
		public readonly DateTime deadline;
		public readonly Action block;
		public readonly long id;
		public bool ignore;
		public readonly string blockName;
		private static long idCounter = 0;

		public TimedBlock (DateTime deadline, Action block, string blockName)
		{
			this.deadline = deadline;
			this.block = block;
			this.blockName = blockName;
			this.id = ++idCounter;
		}

		public int CompareTo(TimedBlock other)
		{
			return this.deadline.CompareTo (other.deadline);
		}

		public override string ToString ()
		{
			return "{TimedBlock: deadLine=" + this.deadline.Ticks + ", id="+ this.id + ", ignore=" + this.ignore + ", block=" + this.blockName + "}";
		}
	}
}

