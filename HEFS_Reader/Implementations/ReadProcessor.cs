using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFS_Reader.Implementations
{
  public class ReadProcessor
  {
    //public static Interfaces.ITimeSeriesOfEnsembleLocations GetDataForWatershedAndTimeRange(Enumerations.Watersheds watershed, DateTime startTime, DateTime endTime, Interfaces.IEnsembleReader dataServiceProvider, string logFilePath, string outputpath)
    //{
    //		if (startTime.Hour != 12) {
    //			//start time must be 12 (actually i think it is supposed to be 10AM
    //			return null;
    //		}
    //		if (endTime.Hour != 12)
    //		{
    //			//end time must be 12 (actually i think it is supposed to be 10AM
    //			return null;
    //		}
    //		if (startTime > endTime) {
    //			// come on guys..
    //			return null;
    //		}
    //		HEFSRequestArgs args = new HEFSRequestArgs();
    //		args.WatershedLocation = watershed;
    //     args.ForecastDate = startTime;
    //     args.Path = outputpath;
    //		Interfaces.ITimeSeriesOfEnsembleLocations output = new TimeSeriesOfEnsembleLocations();
    //		DateTime endTimePlus1 = endTime.AddDays(1.0);
    //		System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    //		while (!startTime.Equals(endTimePlus1))
    //		{
    //			stopwatch.Start();
    //			Interfaces.IWatershedForecast wtshd = dataServiceProvider.Read(args);
    //			if (wtshd != null)
    //			{
    //				output.timeSeriesOfEnsembleLocations.Add(wtshd);
    //			}
    //			else
    //			{
    //				//dont add null data?
    //			}

    //			stopwatch.Stop();
    //			startTime = startTime.AddDays(1.0);
    //       args.ForecastDate = startTime;

    //		}
    //		LogInfo("Reading took: " + stopwatch.Elapsed.ToString(), logFilePath);
    //		return output;
    //	}
    //   private static void LogInfo(string textToappend, string logFile)
    //   {
    //     System.IO.File.AppendAllText(logFile, textToappend);
    //   }
    //   private string StringifyDateTime(DateTime input)
    //	{
    //		string output = "";
    //		output = input.Year.ToString() + StringifyInt(input.Month) + StringifyInt(input.Day) + StringifyInt(input.Hour);
    //		return output;
    //	}
    //	private string StringifyInt(int input)
    //	{
    //		if (input < 10) return "0" + input.ToString();
    //		return input.ToString();
    //	}
    //}
  }
}
