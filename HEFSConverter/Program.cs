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
    const bool SPEEDRUN = true;

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
      //string testfn = @"C:\Temp\SampleDSSFiles\ensemble_V6_100.dss";

      //var en = new DssEnsembleReader();
      //en.ReadDataset(Watersheds.RussianNapa, DateTime.MinValue, DateTime.MaxValue, testfn);

      DSSIO.DSSReader.UseTrainingWheels = false;
      //return;

      Log(NL + NL + "------" + DateTime.Now.ToString() + "-------" + NL + NL);
      Log("Filename".PadRight(FileNameColSize) + Separator +
          "#Ensembles".PadRight(NumEnsColSize) + Separator +
          "Seconds".PadRight(TimeColSize) + Separator +
          "File Size".PadRight(FileSzColSize) + NL);

      if (SPEEDRUN)
      { 
        EndTime = StartTime.AddYears(1);
        Console.WriteLine("SPEED RUN!");
      }

      var provider = new HEFS_CSV_Reader();

      Console.WriteLine("Reading CSV Directory (Parallel)...");
      var rt = Stopwatch.StartNew();
      TimeSeriesOfEnsembleLocations baseWaterShedData = provider.ReadParallel(Watersheds.RussianNapa, StartTime, EndTime, cacheDir);
      rt.Stop();
      Console.WriteLine("Finished reading in " + Math.Round(rt.Elapsed.TotalSeconds) + " seconds.");

      rt = Stopwatch.StartNew();
      Hec.TimeSeries.Ensemble.CsvEnsembleReader r = new Hec.TimeSeries.Ensemble.CsvEnsembleReader(cacheDir);

      //var karl =  r.ReadParallel("RussianNapa", StartTime, EndTime);
      var karl =  r.Read("RussianNapa", StartTime, EndTime);
      rt.Stop();
      Console.WriteLine("Finished reading in " + Math.Round(rt.Elapsed.TotalSeconds) + " seconds.");



      // Let the JITTER settle down with the smallest case
      Console.WriteLine("Warmup time period, results will not be logged.");
      DisableTestReporting = true;

      // I'd like to warmup more, but it's SO FREAKING SLOW
      int warmupEnsembleCount = 1;
      var warmupWatershed = baseWaterShedData.CloneSubset(warmupEnsembleCount);
      for (int i = 0; i < 3; i++)
      {
        WriteAllFormats(warmupWatershed, warmupEnsembleCount);
      }
      DisableTestReporting = false;
      Console.WriteLine("Finished Warmup.");


      int totalCt = baseWaterShedData.Forecasts.Count;

      // Changing this to do the last log-integer count (1000 for our current test) 
      // rather than going to the full ~1800 ensembles
      int steps = (int)Math.Floor(Math.Log(totalCt, 10));

      for (int i = 0; i <= steps; i++)
      {
        // Start with 1, increase by 10x until we get all of them
        int numEnsembles = (int)Math.Pow(10, i);

        Console.WriteLine("Starting test: " + numEnsembles.ToString() + " Ensembles.");

        var watershedSubset = baseWaterShedData.CloneSubset(numEnsembles);
        WriteAllFormats(watershedSubset, numEnsembles);

        ReadAllFormats(numEnsembles, watershedSubset);
      }

      Console.WriteLine("Test complete, log-file written to " + logFile);
    }

    private static void WriteAllFormats(TimeSeriesOfEnsembleLocations waterShedData, int ensembleCount)
    {
      File.AppendAllText(logFile, NL);
      File.AppendAllText(logFile, "---------- Writing " + ensembleCount.ToString() + " Ensembles ----------" + NL);
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
        //Log("---CSV SKIPPED FOR TIME CONSTRAINTS---" + NL);
      }

      // DSS 6/7
      fn = "ensemble_V6_" + ensembleCount + ".dss";
      WriteTimed(fn, ensembleCount, () => DssEnsembleWriter.Write(fn, waterShedData, true, 6));

      fn = "ensemble_V7_" + ensembleCount + ".dss";
      WriteTimed(fn, ensembleCount, () => DssEnsembleWriter.Write(fn, waterShedData, true, 7));

      fn = "ensemble_V7_profiles_" + ensembleCount + ".dss";
      WriteTimed(fn, ensembleCount, () => DssEnsembleWriter.WriteToTimeSeriesProfiles(fn, waterShedData, true, 7));

      // SQLITE
      fn = "ensemble_sqlite_blob_" + ensembleCount + ".db";
      WriteTimed(fn, ensembleCount, () =>
      {
        string connectionString = "Data Source=" + fn + ";Synchronous=Off;Pooling=True;Journal Mode=Off";
        var server = new Reclamation.Core.SQLiteServer(connectionString);
        server.CloseAllConnections();
        SqlBlobEnsembleWriter.Write(server, waterShedData, false);
      });

      fn = "ensemble_sqlite_blob_compressed_" + ensembleCount + ".db";
      WriteTimed(fn, ensembleCount, () =>
      {
        string connectionString = "Data Source=" + fn + ";Synchronous=Off;Pooling=True;Journal Mode=Off";
        var server = new Reclamation.Core.SQLiteServer(connectionString);
        server.CloseAllConnections();
        SqlBlobEnsembleWriter.Write(server, waterShedData, true);
      });


      // Serial HDF5
      fn = "ensemble_serial_1RowPerChunk.h5";
      WriteTimed(fn, ensembleCount, () =>
      {
        using (var h5w = new H5Writer(fn))
          HDF5ReaderWriter.Write(h5w, waterShedData);
      });


      // Parallel HDF5
      foreach (int c in new[] { 1, 10, -1 })
      {
        fn = "ensemble_parallel_" + c.ToString() + "RowsPerChunk.h5";
        WriteTimed(fn, ensembleCount, () =>
        {
          using (var h5w = new H5Writer(fn))
            HDF5ReaderWriter.WriteParallel(h5w, waterShedData, c);
        });
      }
    }

    private static void ReadAllFormats(int ensembleCount, TimeSeriesOfEnsembleLocations csvWaterShedData)
    {
      // TODO - compare validateWatershed data with computed

      File.AppendAllText(logFile, NL);
      File.AppendAllText(logFile, "---------- Reading " + ensembleCount.ToString() + " Ensembles ----------" + NL);
      string fn;

     // TimeSeriesOfEnsembleLocations wshedData=null;
      DateTime startTime = DateTime.MinValue;
      DateTime endTime = DateTime.MaxValue;

      // DSS
      fn = "ensemble_V6_" + ensembleCount + ".dss";
      ReadTimed(fn, csvWaterShedData, () =>
      {
        var reader = new DssEnsembleReader();
        return reader.ReadDataset(Watersheds.RussianNapa, startTime, endTime, fn);
      });

      fn = "ensemble_V7_" + ensembleCount + ".dss";
      ReadTimed(fn, csvWaterShedData, () =>
      {
        var reader = new DssEnsembleReader();
        return reader.ReadDataset(Watersheds.RussianNapa, startTime, endTime, fn);
      });
      
      fn = "ensemble_V7_profiles_" + ensembleCount + ".dss";
      ReadTimed(fn, csvWaterShedData, () =>
      {
        var reader = new DssEnsembleReader();
        return reader.ReadDatasetFromProfiles(Watersheds.RussianNapa, startTime, endTime, fn);
      });

      // SQLITE
      fn = "ensemble_sqlite_blob_" + ensembleCount + ".db";
      ReadTimed(fn, csvWaterShedData, () =>
      {
        var reader = new SqlBlobEnsembleReader();
        return reader.ReadDataset(Watersheds.RussianNapa, startTime, endTime, fn);
      });

      fn = "ensemble_sqlite_blob_compressed_" + ensembleCount + ".db";
      ReadTimed(fn, csvWaterShedData, () =>
      {
        var reader = new SqlBlobEnsembleReader();
        return reader.ReadDataset(Watersheds.RussianNapa, startTime, endTime, fn);
      });

      // Serial HDF5
      fn = "ensemble_serial_1RowPerChunk.h5";
      ReadTimed(fn, csvWaterShedData, () =>
      {
        using (var hr = new H5Reader(fn))
          return  HDF5ReaderWriter.Read(hr);
      });


      // Parallel HDF5
      foreach (int c in new[] { 1, 10, -1 })
      {
        fn = "ensemble_parallel_" + c.ToString() + "RowsPerChunk.h5";
        ReadTimed(fn, csvWaterShedData, () =>
        {
          using (var hr = new H5Reader(fn))
            return HDF5ReaderWriter.Read(hr);
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
        LogWriteResult(filename, ensembleCount, sw.Elapsed);
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
        LogWriteResult(dirName, ensembleCount, sw.Elapsed);
      }
      catch (Exception ex)
      {
        LogWarning(ex.Message);
      }
    }

    private static void ReadTimed(string filename, TimeSeriesOfEnsembleLocations csvWaterShedData, Func<TimeSeriesOfEnsembleLocations> f)
    {
      try
      {
        // Record the amount of time from start->end, including flushing to disk.
        var sw = Stopwatch.StartNew();
        var ensemblesFromDisk = f();
        sw.Stop();
        LogReadResult(filename, ensemblesFromDisk.Forecasts.Count, sw.Elapsed);
        Compare(filename,csvWaterShedData, ensemblesFromDisk);
      }
      catch (Exception ex)
      {
        LogWarning(ex.Message);
      }
    }


    private static void Compare(string filename, TimeSeriesOfEnsembleLocations baseWaterShedData, TimeSeriesOfEnsembleLocations watershed)
    {
      Console.WriteLine();
      Console.Write("checking for any differences..");
      if (!baseWaterShedData.Equals(watershed))
      {
        Console.WriteLine(filename);
        Console.WriteLine("WARNING: watershed read form disk was different!");
        LogWarning("Difference found ");
      }
      else
        Console.WriteLine(" ok.");

      //// compare to reference.
      //var locations = watershed.Forecasts[0].Locations;
      //var refLocations = baseWaterShedData.Forecasts[0].Locations;
      //for (int i = 0; i < locations.Count; i++)
      //{
      //  // .Equals was overriden in the default implementation, 
      //  // but we can't guarantee that for any other implementation....
      //  if (!locations[i].Members.Equals(refLocations[i].Members))
      //    LogWarning("Difference found at location " + locations[i].LocationName);
      //}
    }
    private static void DuplicateCheck(TimeSeriesOfEnsembleLocations baseWaterShedData)
    {
      var hs = new Dictionary<string, int>();
      foreach (var wshed in baseWaterShedData.Forecasts)
      {
        var wsName = wshed.WatershedName;
        foreach (Ensemble ie in wshed.Locations)
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
    static void LogWriteResult(string path, int numEnsemblesToWrite, TimeSpan ts)
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
      Log(path.PadRight(FileNameColSize) + Separator +
          numEnsemblesToWrite.ToString().PadRight(NumEnsColSize) + Separator +
          ts.TotalSeconds.ToString("F2").PadRight(TimeColSize) + Separator +
          BytesToString(size).PadRight(FileSzColSize) + NL);
    }
    static void LogReadResult(string path, int numEnsemblesToWrite, TimeSpan ts)
    {
      if (DisableTestReporting)
        return;

      if (!File.Exists(path))
      {
        LogWarning("File " + path + " was not found!");
        return;
      }

      Log(path.PadRight(FileNameColSize) + Separator +
          numEnsemblesToWrite.ToString().PadRight(NumEnsColSize) + Separator +
          ts.TotalSeconds.ToString("F2").PadRight(TimeColSize) + Separator +
          "(Reading)".PadRight(FileSzColSize) + NL);
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
