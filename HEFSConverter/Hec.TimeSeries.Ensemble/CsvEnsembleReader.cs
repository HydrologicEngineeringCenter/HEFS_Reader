using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;


namespace Hec.TimeSeries.Ensemble
{
  public class CsvEnsembleReader : EnsembleReader
  {

    string path; // path to csv files
    public CsvEnsembleReader(string path)
    {
      this.path = path;
    }

    // Allegedly threadsafe
    // https://docs.microsoft.com/en-us/dotnet/api/system.console?redirectedfrom=MSDN&view=netframework-4.8
    static bool DebugMode = false;
    static void Log(string msg)
    {
      if (DebugMode)
        Console.WriteLine(msg);
    }
    static void LogWarning(string msg) => Console.WriteLine("Warning: " + msg);
    /// <summary>
    /// Reads list of Forecast
    /// </summary>
    /// <param name="watershedName"></param>
    /// <param name="forecastDate"></param>
    /// <returns></returns>
    Forecast[] Read(string watershedName, DateTime forecastDate)
    {
      //https://www.cnrfc.noaa.gov/csv/2019092312_RussianNapa_hefs_csv_hourly.zip

      string fileName = forecastDate.ToString("yyyyMMddhh") + "_";
      fileName += watershedName;
      fileName += "_hefs_csv_hourly";

      string csvFileName = Path.Combine(path, fileName + ".csv");
      if (File.Exists(csvFileName))
      {
        Log("Found " + csvFileName + " in cache.  Reading...");

        var  x = CsvParser.Parse(File.ReadAllText(csvFileName),
           forecastDate, watershedName);

        return w;
      }
      else
      {
        LogWarning("Warning: " + csvFileName + " not found, skipping");
        return null;
      }
    }

    public Watershed Read(string watershedName, DateTime startDate, DateTime endDate)
    {
      if (startDate.Hour != 12)
      {
        //start time must be 12 (actually i think it is supposed to be 10AM
        return null;
      }
      if (endDate.Hour != 12)
      {
        //end time must be 12 (actually i think it is supposed to be 10AM
        return null;
      }
      if (startDate > endDate)
      {
        // come on guys..
        return null;
      }

      var output = new Watershed(watershedName);

      // Each forecast is one day
      int numTotal = (int)Math.Round((endDate - startDate).TotalDays) + 1;

      Parallel.For(0, numTotal, i =>
      {
        DateTime day = startDate.AddDays(i);

        // Seems threadsafe at a glance
        Forecast[] F = Read(args);
        if (wtshd != null)
        {
          lock (output)
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

