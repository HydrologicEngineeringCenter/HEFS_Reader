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

namespace Hec.TimeSeries.Ensemble
{
  public class EnsembleTester
  {
    // So I can test this reliably at work....
    const bool SPEEDRUN = true;
    public static string CacheDir = @"C:\Temp\hefs_cache";
    static string logFile = "Ensemble_testing.log";

    // Global start/end times for the Russian River CSV dataset
    static DateTime StartTime = new DateTime(2013, 11, 1, 12, 0, 0);
    static DateTime EndTime = new DateTime(2013, 11, 5, 12, 0, 0);
    //static DateTime EndTime = new DateTime(2017, 11, 1, 12, 0, 0);

    static string NL = Environment.NewLine;
    const string Separator = " | ";
    const int FileNameColSize = 40;
    const int NumEnsColSize = 10;
    const int TimeColSize = 10;
    const int FileSzColSize = 10;

    static bool DisableTestReporting = false;

    static void Main(string[] args)
    {
      DSSIO.DSSReader.UseTrainingWheels = false;

      Log(NL + NL + "------" + DateTime.Now.ToString() + "-------" + NL + NL);
      Log("Filename".PadRight(FileNameColSize) + Separator +
          "#Ensembles".PadRight(NumEnsColSize) + Separator +
          "Seconds".PadRight(TimeColSize) + Separator +
          "File Size".PadRight(FileSzColSize) + NL);

      var watershedNames = new string[] { "RussianNapa", "EastSierra", "FeatherYuba" };
      Watershed[] baseWaterShedData = ReadCsvFiles(watershedNames);

      //Warmup(baseWaterShedData);

      Console.WriteLine("Starting test:");

      int count = 0;
      foreach (var w in baseWaterShedData)
      {
        bool delete = count == 0;
        WriteAllFormats(w, delete);
        count++;
      }

      count = 0;
      foreach (var w in baseWaterShedData)
      {
        count++;
        //ReadAllFormats(w, count == 1);
      }

      Console.WriteLine("Test complete, log-file written to " + logFile);
    }

    private static void Warmup(Watershed[] baseWaterShedData)
    {
      // Let the JITTER settle down with the smallest case
      Console.WriteLine("Warmup time period, results will not be logged.");
      DisableTestReporting = true;

      // I'd like to warmup more, but it's SO FREAKING SLOW
      int warmupEnsembleCount = 1;
      foreach (Watershed ws in baseWaterShedData)
      {
        var warmupWatershed = ws.CloneSubset(warmupEnsembleCount);
        WriteAllFormats(warmupWatershed, false);
      }
      DisableTestReporting = false;
      Console.WriteLine("Finished Warmup.");
    }

    private static Watershed[] ReadCsvFiles(string[] watersheds)
    {
      List<Watershed> rval = new List<Watershed>(watersheds.Length);
      CsvEnsembleReader csvReader = new CsvEnsembleReader(CacheDir);
      Console.WriteLine("Reading CSV Directory...");
      var rt = Stopwatch.StartNew();
      foreach (var wsName in watersheds)
      {
        var ws= csvReader.ReadParallel(wsName, StartTime, EndTime);
        rval.Add(ws);
      }
      
      rt.Stop();
      Console.WriteLine("Finished reading csv's in " + Math.Round(rt.Elapsed.TotalSeconds) + " seconds.");
      return rval.ToArray();
    }

    private static void WriteAllFormats(Watershed waterShedData, bool delete )
    {
      File.AppendAllText(logFile, NL);
      string fn, dir;
      string tag = "round3"; 
      // DSS 6/7
      fn = "ensemble_V7_" + tag + ".dss";
      if( delete) File.Delete(fn);
      WriteTimed(fn, tag, () => DssEnsemble.Write(fn, waterShedData));

      fn = "ensemble_V7_profiles_" + tag + ".dss";
      if (delete) File.Delete(fn);
      WriteTimed(fn, tag, () => DssEnsemble.WriteToTimeSeriesProfiles(fn, waterShedData));

      
      // SQLITE
      fn = "ensemble_sqlite_blob_compressed_" + tag + ".pdb";
      if (delete) File.Delete(fn);
      WriteTimed(fn, tag, () =>
      {
        string connectionString = "Data Source=" + fn + ";Synchronous=Off;Pooling=True;Journal Mode=Off";
        var server = new Reclamation.Core.SQLiteServer(connectionString);
        server.CloseAllConnections();
        SqLiteEnsemble.Write(server, waterShedData, true);
      });


      // Serial HDF5
      //fn = "ensemble_serial_1RowPerChunk.h5";
      //WriteTimed(fn, ensembleCount, () =>
      //{
      //  using (var h5w = new H5Writer(fn))
      //    HDF5ReaderWriter.Write(h5w, waterShedData);
      //});


      //// Parallel HDF5
      //foreach (int c in new[] { 1, 10, -1 })
      //{
      //  fn = "ensemble_parallel_" + c.ToString() + "RowsPerChunk.h5";
      //  WriteTimed(fn, ensembleCount, () =>
      //  {
      //    using (var h5w = new H5Writer(fn))
      //      HDF5ReaderWriter.WriteParallel(h5w, waterShedData, c);
      //  });
      //}
    }

    private static void ReadAllFormats(int ensembleCount, TimeSeriesOfEnsembleLocations csvWaterShedData)
    {
      // TODO - compare validateWatershed data with computed

      //File.AppendAllText(logFile, NL);
      //File.AppendAllText(logFile, "---------- Reading " + ensembleCount.ToString() + " Ensembles ----------" + NL);
      //string fn;

      //// TimeSeriesOfEnsembleLocations wshedData=null;
      //DateTime startTime = DateTime.MinValue;
      //DateTime endTime = DateTime.MaxValue;

      //// DSS
      //fn = "ensemble_V6_" + ensembleCount + ".dss";
      //ReadTimed(fn, csvWaterShedData, () =>
      //{
      //  var reader = new DssEnsembleReader();
      //  return reader.ReadDataset(Watersheds.RussianNapa, startTime, endTime, fn);
      //});

      //fn = "ensemble_V7_" + ensembleCount + ".dss";
      //ReadTimed(fn, csvWaterShedData, () =>
      //{
      //  var reader = new DssEnsembleReader();
      //  return reader.ReadDataset(Watersheds.RussianNapa, startTime, endTime, fn);
      //});

      //fn = "ensemble_V7_profiles_" + ensembleCount + ".dss";
      //ReadTimed(fn, csvWaterShedData, () =>
      //{
      //  var reader = new DssEnsembleReader();
      //  return reader.ReadDatasetFromProfiles(Watersheds.RussianNapa, startTime, endTime, fn);
      //});

      //// SQLITE
      //fn = "ensemble_sqlite_blob_" + ensembleCount + ".db";
      //ReadTimed(fn, csvWaterShedData, () =>
      //{
      //  var reader = new SqlBlobEnsembleReader();
      //  return reader.ReadDataset(Watersheds.RussianNapa, startTime, endTime, fn);
      //});

      //fn = "ensemble_sqlite_blob_compressed_" + ensembleCount + ".db";
      //ReadTimed(fn, csvWaterShedData, () =>
      //{
      //  var reader = new SqlBlobEnsembleReader();
      //  return reader.ReadDataset(Watersheds.RussianNapa, startTime, endTime, fn);
      //});

      //// Serial HDF5
      //fn = "ensemble_serial_1RowPerChunk.h5";
      //ReadTimed(fn, csvWaterShedData, () =>
      //{
      //  using (var hr = new H5Reader(fn))
      //    return HDF5ReaderWriter.Read(hr);
      //});


      //// Parallel HDF5
      //foreach (int c in new[] { 1, 10, -1 })
      //{
      //  fn = "ensemble_parallel_" + c.ToString() + "RowsPerChunk.h5";
      //  ReadTimed(fn, csvWaterShedData, () =>
      //  {
      //    using (var hr = new H5Reader(fn))
      //      return HDF5ReaderWriter.Read(hr);
      //  });
      //}

    }


    // Writer helpers
    private static void WriteTimed(string filename, string tag, Action CreateFile)
    {
      try
      {
        //File.Delete(filename);

        // Record the amount of time from start->end, including flushing to disk.
        var sw = Stopwatch.StartNew();

        CreateFile();

        sw.Stop();
        LogWriteResult(filename, tag, sw.Elapsed);
      }
      catch (Exception ex)
      {
        LogWarning(ex.Message);
      }
    }
    private static void WriteDirectoryTimed(string dirName, string tag, Action CreateFile)
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
        LogWriteResult(dirName, tag, sw.Elapsed);
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
        Compare(filename, csvWaterShedData, ensemblesFromDisk);
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
      //var hs = new Dictionary<string, int>();
      //foreach (var wshed in baseWaterShedData.Forecasts)
      //{
      //  var wsName = wshed.WatershedName;
      //  foreach (Ensemble ie in wshed.Locations)
      //  {
      //    // This is being treated like a unique entity...
      //    string ensemblePath = ie.LocationName + "|" + ie.IssueDate.Year.ToString() + "_" + ie.IssueDate.DayOfYear.ToString();
      //    if (hs.ContainsKey(ensemblePath))
      //    {
      //      Console.WriteLine("Duplicate found.");
      //      int ct = hs[ensemblePath];
      //      hs[ensemblePath] = ct + 1;
      //    }
      //    else
      //    {
      //      hs.Add(ensemblePath, 1);
      //    }

      //  }
      //}
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
    static void LogWriteResult(string path, string tag, TimeSpan ts)
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
          tag.ToString().PadRight(NumEnsColSize) + Separator +
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
