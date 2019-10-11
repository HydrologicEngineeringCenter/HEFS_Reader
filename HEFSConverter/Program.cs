using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using DSSIO;
using HEFS_Reader.Interfaces;
using HEFS_Reader.Implementations;
using HEFS_Reader.Enumerations;
using H5Assist;

namespace HEFSConverter
{
  class Program
  {
    // So I can test this reliably at work....
    const bool SKIP_SLOW_TESTS = false;

    static string cacheDir = @"C:\Temp\hefs_cache";
    static string logFile = "Ensemble_testing.log";

    // Global start/end times for the Russian River CSV dataset
    static DateTime StartTime = new DateTime(2013, 11, 1, 12, 0, 0);
    static DateTime EndTime = new DateTime(2017, 11, 1, 12, 0, 0);

    static string NL = Environment.NewLine;
    const string Separator = " | ";
    const int FileNameColSize = 40;
    const int NumEnsColSize = 10;
    const int TimeColSize = 10;
    const int FileSzColSize = 10;

    static bool DisableTestReporting = false;

    static void Main(string[] args)
    {
      Log(NL + NL + "------" + DateTime.Now.ToString() + "-------" + NL + NL);
      Log("Filename".PadRight(FileNameColSize) + Separator +
          "#Ensembles".PadRight(NumEnsColSize) + Separator +
          "Seconds".PadRight(TimeColSize) + Separator +
          "File Size".PadRight(FileSzColSize) + NL);

      var provider = new HEFS_CSV_Reader();

      //ITimeSeriesOfEnsembleLocations baseWaterShedData = provider.ReadDataset(Watersheds.RussianNapa, StartTime, EndTime, cacheDir);

      Console.WriteLine("Reading CSV Directory (Parallel)...");
      ITimeSeriesOfEnsembleLocations baseWaterShedData = provider.ReadParallel(Watersheds.RussianNapa, StartTime, EndTime, cacheDir);
      Console.WriteLine("Finished reading in " + Math.Round(provider.ReadTime.TotalSeconds) + " seconds.");

      // Let the JITTER settle down with the smallest case
      Console.WriteLine("Warmup time period, results will not be logged.");
      DisableTestReporting = true;

      // I'd like to warmup more, but it's SO FREAKING SLOW
      int warmupEnsembleCount = 1;
      var warmupWatershed = baseWaterShedData.CloneSubset(warmupEnsembleCount);
      //var confirmed = provider.ReadDataset(Watersheds.RussianNapa, StartTime, new DateTime(2013, 11, 1, 12, 0, 0), cacheDir);
      for (int i = 0; i < 3; i++)
      {
        WriteAllFormats(warmupWatershed, warmupEnsembleCount, StartTime);
      }
      DisableTestReporting = false;
      Console.WriteLine("Finished Warmup.");


      int totalCt = baseWaterShedData.Forecasts.Count;
      int steps = (int)Math.Ceiling(Math.Log(totalCt, 10));

      for (int i = 0; i <= steps; i++)
      {
        // Start with 1, increase by 10x until we get all of them
        int numEnsembles = (int)Math.Pow(10, i);

        Console.WriteLine("Starting test: " + numEnsembles.ToString() + " Ensembles.");

        var watershedSubset = baseWaterShedData.CloneSubset(numEnsembles);
        WriteAllFormats(watershedSubset, numEnsembles, StartTime);

        // READ

        /*
        var fn = "ensemble_V7" + numEnsembles + ".dss";
        HEFS_Reader.Interfaces.IEnsembleReader dssReader = new DssEnsembleReader();
        var watershed = dssReader.ReadDataset(Watersheds.RussianNapa, startTime, endTime, fn);
        LogInfo(fn, numEnsembles, ((HEFS_Reader.Interfaces.ITimeable)dssReader).ReadTimeInMilliSeconds / 1000);//potentially unsafe action.

        Compare(baseWaterShedData, watershed);
        */
      }
      

      Console.WriteLine("Test complete, log-file written to " + logFile);
    }

    private static void WriteAllFormats(ITimeSeriesOfEnsembleLocations waterShedData, int ensembleCount, DateTime startTime)
    {
      File.AppendAllText(logFile, NL);
      File.AppendAllText(logFile, startTime.ToString() + ", Count = " + ensembleCount.ToString() + NL);
      string fn, dir;

      // CSV

      if (false)
      {
        dir = Path.Combine(Directory.GetCurrentDirectory(), "csv_out_" + ensembleCount);
        WriteDirectoryTimed(dir, ensembleCount, () =>
        {
          HEFS_CSV_Writer w = new HEFS_CSV_Writer();
          w.Write(waterShedData, dir);
        });
      }
      else
      {
        Log("---CSV SKIPPED FOR TIME CONSTRAINTS---" + NL);
      }

      // DSS 6/7
      if (!(SKIP_SLOW_TESTS && ensembleCount >= 1000))
      {
        fn = "ensemble_V6" + ensembleCount + ".dss";
        WriteTimed(fn, ensembleCount, () => DssEnsembleWriter.Write(fn, waterShedData, true, 6));

        fn = "ensemble_V7" + ensembleCount + ".dss";
        WriteTimed(fn, ensembleCount, () => DssEnsembleWriter.Write(fn, waterShedData, true, 7));
      }
      else
      {
        Log("---DSS SKIPPED FOR TIME CONSTRAINTS---" + NL);
      }

      // SQLITE
      if (!(SKIP_SLOW_TESTS && ensembleCount >= 1000))
      {
        fn = "ensemble_sqlite" + ensembleCount + ".db";
        WriteTimed(fn, ensembleCount, () =>
        {
          string connectionString = "Data Source=" + fn + ";Synchronous=Off;Pooling=True;Journal Mode=Off";
          Reclamation.Core.SQLiteServer server = new Reclamation.Core.SQLiteServer(connectionString);
          SqlEnsembleWriter.Write(server, waterShedData);
          //ts = SqlEnsembleWriter.WriteToDatabase(server, startTime, endTime, true, cacheDir);
          server.CloseAllConnections();
        });
      }
      else
      {
        Log("---SQLITE S1 SKIPPED FOR TIME CONSTRAINTS---" + NL);
      }

      fn = "ensemble_sqlite_blob" + ensembleCount + ".db";
      WriteTimed(fn, ensembleCount, () =>
      {
        string connectionString = "Data Source=" + fn + ";Synchronous=Off;Pooling=True;Journal Mode=Off";
        var server = new Reclamation.Core.SQLiteServer(connectionString);
        server.CloseAllConnections();
        SqlBlobEnsemble.Write(server, waterShedData, false);
      });

      fn = "ensemble_sqlite_blob_compressed" + ensembleCount + ".db";
      WriteTimed(fn, ensembleCount, () =>
      {
        string connectionString = "Data Source=" + fn + ";Synchronous=Off;Pooling=True;Journal Mode=Off";
        var server = new Reclamation.Core.SQLiteServer(connectionString);
        server.CloseAllConnections();
        SqlBlobEnsemble.Write(server, waterShedData, true);
      });


      // Serial HDF5
      fn = "ensemble_serial_1RowPerChunk.h5";
      WriteTimed(fn, ensembleCount, () =>
      {
        using (var h5w = new H5Writer(fn))
          HDF5ReaderWriter.Write(h5w, waterShedData);
      });


      // Parallel HDF5
      foreach (int c in new[] { 1, 5, 10, -1 })
      {
        fn = "ensemble_parallel_" + c.ToString() + "RowsPerChunk.h5";
        WriteTimed(fn, ensembleCount, () =>
        {
          using (var h5w = new H5Writer(fn))
            HDF5ReaderWriter.WriteParallel(h5w, waterShedData, c);
        });
      }
    }


    // Writer helpers
    private static void WriteTimed(string filename, int ensembleCount, Action CreateFile)
    {
      try
      {
        File.Delete(filename);

        // Record the amount of time from start->end, including flushing to disk.
        var sw = Stopwatch.StartNew();

        CreateFile();

        sw.Stop();
        LogResult(filename, ensembleCount, sw.Elapsed);
      }
      catch (Exception ex)
      {
        LogWarning(ex.Message);
      }
    }
    private static void WriteDirectoryTimed(string dirName, int ensembleCount, Action CreateFile)
    {
      try
      {
        // Clear it
        if (Directory.Exists(dirName))
          Directory.Delete(dirName, true);

        Directory.CreateDirectory(dirName);

        // Record the amount of time from start->end, including flushing to disk.
        var sw = Stopwatch.StartNew();

        CreateFile();

        sw.Stop();
        LogResult(dirName, ensembleCount, sw.Elapsed);
      }
      catch (Exception ex)
      {
        LogWarning(ex.Message);
      }
    }


    private static void Compare(ITimeSeriesOfEnsembleLocations baseWaterShedData, ITimeSeriesOfEnsembleLocations watershed)
    {
      // compare to reference.
      var locations = watershed.Forecasts[0].Locations;
      var refLocations = baseWaterShedData.Forecasts[0].Locations;
      for (int i = 0; i < locations.Count; i++)
      {
        // .Equals was overriden in the default implementation, 
        // but we can't guarantee that for any other implementation....
        if (!locations[i].Members.Equals(refLocations[i].Members))
          LogWarning("Difference found at location " + locations[i].LocationName);
      }
    }
    private static void DuplicateCheck(ITimeSeriesOfEnsembleLocations baseWaterShedData)
    {
      var hs = new Dictionary<string, int>();
      foreach (var wshed in baseWaterShedData.Forecasts)
      {
        var wsName = wshed.WatershedName;
        foreach (IEnsemble ie in wshed.Locations)
        {
          // This is being treated like a unique entity...
          string ensemblePath = ie.LocationName + "|" + ie.IssueDate.Year.ToString() + "_" + ie.IssueDate.DayOfYear.ToString();
          if (hs.ContainsKey(ensemblePath))
          {
            Console.WriteLine("Duplicate found.");
            int ct = hs[ensemblePath];
            hs[ensemblePath] = ct + 1;
          }
          else
          {
            hs.Add(ensemblePath, 1);
          }

        }
      }
    }


    // Logging helpers
    static void Log(string msg)
    {
      // So we can dump to a file, console-write, etc.

      // Note this isn't a file-lock, it's an object lock. Should work for our purposes 
      // if multiple threads are trying to write at once.
      lock (logFile)
        File.AppendAllText(logFile, msg);
    }
    static void LogWarning(string msg)
    {
      lock (logFile)
        File.AppendAllText(logFile, "WARNING: " + msg);
    }
    static void LogResult(string path, int numEnsemblesToWrite, TimeSpan ts)
    {
      if (DisableTestReporting)
        return;

      long size;

      if (File.Exists(path))
      {
        FileInfo fi = new FileInfo(path);
        size = fi.Length;
      }
      else if (Directory.Exists(path))
      {
        size = GetDirectorySize(path);
        path = path.Split('\\').Last();
      }
      else
      {
        LogWarning("Path '" + path + "' does not exist on the file system.");
        return;
      }

      double mb = size / 1024.0 / 1024.0;
      double mbs = mb / ts.TotalSeconds;

      Log(path.PadRight(FileNameColSize) + Separator +
          numEnsemblesToWrite.ToString().PadRight(NumEnsColSize) + Separator +
          ts.TotalSeconds.ToString("F2").PadRight(TimeColSize) + Separator +
          BytesToString(size).PadRight(FileSzColSize) + NL);

      //mbs.ToString("F2") + " MB/sec" + NL;

    }

    // Other helpers
    static long GetDirectorySize(string p)
    {
      // 1. Get array of all file names.
      string[] a = Directory.GetFiles(p, "*.*");

      // 2. Calculate total bytes of all files in a loop.
      long b = 0;
      foreach (string name in a)
      {
        // 3. Use FileInfo to get length of each file.
        FileInfo info = new FileInfo(name);
        b += info.Length;
      }

      // 4. Return total size
      return b;
    }

    //static DateTime GetEndTime(int index)
    //{
    //  if (index == 1)
    //    return new DateTime(2013, 11, 1, 12, 0, 0);
    //  if (index == 10)
    //    return new DateTime(2013, 11, 11, 12, 0, 0);
    //  if (index == 100)
    //    return new DateTime(2014, 2, 8, 12, 0, 0);
    //  if (index == 1000)
    //    return new DateTime(2016, 7, 29, 12, 0, 0);

    //  return new DateTime(2017, 11, 1, 12, 0, 0);
    //}

    /// <summary>
    /// https://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
    /// </summary>
    /// <param name="byteCount"></param>
    /// <returns></returns>
    static string BytesToString(long byteCount)
    {
      string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
      if (byteCount == 0)
        return "0" + suf[0];

      long bytes = Math.Abs(byteCount);
      int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
      double num = Math.Round(bytes / Math.Pow(1024, place), 2);
      return (Math.Sign(byteCount) * num).ToString() + suf[place];
    }

  }
}
