using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFS_Reader.Implementations
{
	public class TimeSeriesOfEnsembles
	{
		public IList<IList<Interfaces.IEnsemble>> getDataForWatershedAndTimeRange(Enumerations.Watersheds watershed, DateTime startTime, DateTime endTime)
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
			args.date = StringifyDateTime(startTime);
			List<IList<Interfaces.IEnsemble>> output = new List<IList<Interfaces.IEnsemble>>();
			HEFS_Downloader dl = new HEFS_Downloader();
			if (dl.FetchData(args))
			{
				output.Add(HEFS_Reader.readData(dl.Response, startTime));
			}
			
			while (!startTime.Equals(endTime))
			{
				startTime = startTime.AddDays(1.0);
				args.date = StringifyDateTime(startTime);
				if (dl.FetchData(args))
				{
					output.Add(HEFS_Reader.readData(dl.Response, startTime));
				}
			}
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
