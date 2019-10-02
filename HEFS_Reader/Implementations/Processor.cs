using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFS_Reader.Implementations
{
	public class Processor
	{
		public static Interfaces.ITimeSeriesOfEnsembleLocations GetDataForWatershedAndTimeRange(Enumerations.Watersheds watershed, DateTime startTime, DateTime endTime, Interfaces.IEnsembleReader dataServiceProvider)
		{
			if (startTime.Hour != 12) {
				//start time must be 12 (actually i think it is supposed to be 10AM
				return null;
			}
			if (endTime.Hour != 12)
			{
				//end time must be 12 (actually i think it is supposed to be 10AM
				return null;
			}
			if (startTime > endTime) {
				// come on guys..
				return null;
			}
			HEFSRequestArgs args = new HEFSRequestArgs();
			args.location = watershed;
      args.date = startTime;
			Interfaces.ITimeSeriesOfEnsembleLocations output = new TimeSeriesOfEnsembleLocations();
			DateTime endTimePlus1 = endTime.AddDays(1.0);
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			while (!startTime.Equals(endTimePlus1))
			{
				stopwatch.Start();
				Interfaces.IWatershed wtshd = dataServiceProvider.Read(args);
				if (wtshd != null)
				{
					output.timeSeriesOfEnsembleLocations.Add(wtshd);
				}
				else
				{
					//dont add null data?
				}
				
				stopwatch.Stop();
				startTime = startTime.AddDays(1.0);
        args.date = startTime;
				
			}
			Console.WriteLine("Reading took: " + stopwatch.Elapsed.ToString());
			return output;
		}
		private string StringifyDateTime(DateTime input)
		{
			string output = "";
			output = input.Year.ToString() + StringifyInt(input.Month) + StringifyInt(input.Day) + StringifyInt(input.Hour);
			return output;
		}
		private string StringifyInt(int input)
		{
			if (input < 10) return "0" + input.ToString();
			return input.ToString();
		}
	}
}
