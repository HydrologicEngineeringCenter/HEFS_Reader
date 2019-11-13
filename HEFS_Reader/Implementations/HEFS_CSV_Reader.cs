using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HEFS_Reader.Enumerations;
using HEFS_Reader.Interfaces;

namespace HEFS_Reader.Implementations
{
  public class HEFS_CSV_Reader : IEnsembleReader
  {
    // Allegedly threadsafe
    // https://docs.microsoft.com/en-us/dotnet/api/system.console?redirectedfrom=MSDN&view=netframework-4.8
    static bool DebugMode = false;
    static void Log(string msg)
    {
      if (DebugMode)
        Console.WriteLine(msg);
    }
    static void LogWarning(string msg) => Console.WriteLine("Warning: " + msg);

    public HEFS_CSV_Reader()
    {
      //_cacheDirectory = Path.GetTempPath();
    }

    public WatershedForecast Read(HEFSRequestArgs args)
    {
      //https://www.cnrfc.noaa.gov/csv/2019092312_RussianNapa_hefs_csv_hourly.zip

      string fileName = args.ForecastDate.ToString("yyyyMMddhh") + "_";
      fileName += args.WatershedLocation.ToString();
      fileName += "_hefs_csv_hourly";

      string csvFileName = Path.Combine(args.Path, fileName + ".csv");
      if (File.Exists(csvFileName))
      {
        Log("Found " + csvFileName + " in cache.  Reading...");

        WatershedForecast w = HEFS_CSV_Parser.ParseCSVData(File.ReadAllText(csvFileName),
           args.ForecastDate, args.WatershedLocation);

        return w;
      }
      else
      {
        LogWarning("Warning: " + csvFileName + " not found, skipping");
        return null;
      }
    }

    public TimeSeriesOfEnsembleLocations ReadDataset(Watersheds watershed, DateTime start, DateTime end, string Path)
    {
      var st = Stopwatch.StartNew();
      if (start.Hour != 12)
      {
        //start time must be 12 (actually i think it is supposed to be 10AM
        return null;
      }
      if (end.Hour != 12)
      {
        //end time must be 12 (actually i think it is supposed to be 10AM
        return null;
      }
      if (start > end)
      {
        // come on guys..
        return null;
      }

      HEFSRequestArgs args = new HEFSRequestArgs();
      args.WatershedLocation = watershed;
      args.ForecastDate = start;
      args.Path = Path;
      
      TimeSeriesOfEnsembleLocations output = new TimeSeriesOfEnsembleLocations();

      DateTime current = start;
      DateTime endTimePlus1 = end.AddDays(1.0);

      while (!current.Equals(endTimePlus1))
      {
        WatershedForecast wtshd = Read(args);
        if (wtshd != null)
        {
          output.Forecasts.Add(wtshd);
        }
        else
        {
          //dont add null data?
        }
        current = current.AddDays(1.0);
        args.ForecastDate = current;
      }

      st.Stop();
      return output;
    }


    public TimeSeriesOfEnsembleLocations ReadParallel(Watersheds watershed, DateTime start, DateTime end, string Path)
    {
      if (start.Hour != 12)
      {
        //start time must be 12 (actually i think it is supposed to be 10AM
        return null;
      }
      if (end.Hour != 12)
      {
        //end time must be 12 (actually i think it is supposed to be 10AM
        return null;
      }
      if (start > end)
      {
        // come on guys..
        return null;
      }

      var output = new TimeSeriesOfEnsembleLocations();

      // Each forecast is one day
      int numTotal = (int)Math.Round((end - start).TotalDays) + 1;

      Parallel.For(0, numTotal, i =>
      {
        DateTime day = start.AddDays(i);

        HEFSRequestArgs args = new HEFSRequestArgs();
        args.WatershedLocation = watershed;
        args.ForecastDate = day;
        args.Path = Path;

        // Seems threadsafe at a glance
        WatershedForecast wtshd = Read(args);
        if (wtshd != null)
        {
          lock(output)
            output.Forecasts.Add(wtshd);
        }
        else
        {
          //dont add null data?
        }

      });

      // I don't know if watershed sorting actually matters here...?
      // Issue-date seems like it should be one level higher?
      output.SortWatersheds();

      return output;
    }
  }
}

