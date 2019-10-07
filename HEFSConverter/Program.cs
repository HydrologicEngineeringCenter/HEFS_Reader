using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using DSSIO;
using HEFS_Reader.Implementations;
using HEFS_Reader.Enumerations;

namespace HEFSConverter
{
  class Program
  {
    static string cacheDir = @"C:\Temp\hefs_cache";

    static DateTime GetEndTime(int index)
    {
      if (index == 1)
        return new DateTime(2013, 11, 1, 12, 0, 0);
      if (index == 10)
        return new DateTime(2013, 11, 11, 12, 0, 0);
      if (index == 100)
        return new DateTime(2014, 2, 8, 12, 0, 0);
      if (index == 1000)
        return new DateTime(2016, 7, 29, 12, 0, 0);

      return new DateTime(2017, 11, 1, 12, 0, 0);

    }
    static string logFile = "Ensemble_testing.log";
    static void Main(string[] args)
    {
      int numEnsembles = 1;

      File.AppendAllText(logFile, "\n\n------" + DateTime.Now.ToString() + "-------\n\n");
      File.AppendAllText(logFile, "filename, numEnsembles,seconds, filesize\n");

      while (numEnsembles <= 10000)
      {
        DateTime startTime = new DateTime(2013, 11, 1, 12, 0, 0);

        // WRITE

        WriteToMultipleFormats(numEnsembles, startTime);

        // READ
        DateTime endTime = GetEndTime(numEnsembles);

        
        var fn = "ensemble_V7" + numEnsembles + ".dss";
        var watershed = DssEnsembleReader.Read(fn, Watersheds.RussianNapa, startTime, endTime,out TimeSpan ts);
        LogInfo(fn, numEnsembles, ts.TotalSeconds);


        numEnsembles *= 10;
      }
    }

    private static void WriteToMultipleFormats(int numEnsemblesToWrite, DateTime startTime)
    {
      TimeSpan ts;
      DateTime endTime = GetEndTime(numEnsemblesToWrite);
    //  startTime = new DateTime(2016, 11, 1, 12, 0, 0);
     // endTime = new DateTime(2017, 2, 28, 12, 0, 0);

      var provider = new HEFS_Reader.Implementations.HEFS_DataServiceProvider();
      var dssProvider = new HEFSConverter.DssEnsembleReader();
      var waterShedData =
      provider.ReadDataset(Watersheds.RussianNapa, startTime, endTime, cacheDir);


      File.AppendAllText(logFile, "\n");
      File.AppendAllText(logFile, startTime.ToString() + " -->  " + endTime.ToString() + "\n");

      HEFS_Reader.Implementations.HEFS_CSV_Writer w = new HEFS_CSV_Writer();
      var dir = Path.Combine(Directory.GetCurrentDirectory(), "csv_out_" + numEnsemblesToWrite);
      if (Directory.Exists(dir))
        Directory.Delete(dir, true);
      Directory.CreateDirectory(dir);
      ts = w.Write(waterShedData, dir);
      LogInfo(dir, numEnsemblesToWrite, ts.TotalSeconds, true);

      var fn = "ensemble_V7" + numEnsemblesToWrite + ".dss";
      ts = DssEnsembleWriter.Write(fn, waterShedData, true, 7);
      LogInfo(fn, numEnsemblesToWrite, ts.TotalSeconds);

      fn = "ensemble_V6" + numEnsemblesToWrite + ".dss";
      ts = DssEnsembleWriter.Write(fn, waterShedData, true, 6);
      LogInfo(fn, numEnsemblesToWrite, ts.TotalSeconds);


      fn = "ensemble_sqlite" + numEnsemblesToWrite + ".db";
      File.Delete(fn);
      var connectionString = "Data Source=" + fn + ";Synchronous=Off;Pooling=True;Journal Mode=Off";
      Reclamation.Core.SQLiteServer server = new Reclamation.Core.SQLiteServer(connectionString);
      //ts = SqlEnsembleWriter.WriteToDatabase(server, startTime, endTime, true, cacheDir);
      ts = SqlEnsembleWriter.Write(server, waterShedData);
      server.CloseAllConnections();
      LogInfo(fn, numEnsemblesToWrite, ts.TotalSeconds);

      fn = "ensemble_sqlite_blob" + numEnsemblesToWrite + ".db";
      File.Delete(fn);
      connectionString = "Data Source=" + fn + ";Synchronous=Off;Pooling=True;Journal Mode=Off";
      server = new Reclamation.Core.SQLiteServer(connectionString);
      server.CloseAllConnections();
      ts = SqlBlobEnsemble.Write(server, waterShedData, false);
      LogInfo(fn, numEnsemblesToWrite, ts.TotalSeconds);

      fn = "ensemble_sqlite_blob_compressed" + numEnsemblesToWrite + ".db";
      File.Delete(fn);
      connectionString = "Data Source=" + fn + ";Synchronous=Off;Pooling=True;Journal Mode=Off";
      server = new Reclamation.Core.SQLiteServer(connectionString);
      server.CloseAllConnections();
      ts = SqlBlobEnsemble.Write(server, waterShedData, true);
      LogInfo(fn, numEnsemblesToWrite, ts.TotalSeconds);

      //  CreateSqLiteBlobDatabaseOfEnsembles("sqlite_blob_ensemble_float.pdb", startTime, endTime);
    }

    static void LogInfo(string path, int numEnsemblesToWrite, double seconds, bool isDir = false)
    {
      long size = 0;
      if (isDir)
      {
        size = GetDirectorySize(path);
        path = path.Split('\\').Last();
      }
      else
      {
        FileInfo fi = new FileInfo(path);
        size = fi.Length;

      }
      double mb =  size/ 1024.0 / 1024.0;
      double mbs = mb / seconds;

      string rval = path + ", " + numEnsemblesToWrite + ", " + seconds.ToString("F2") + " s ," + BytesToString(size) + ", " + mbs.ToString("F2") + " mb/seconds\n";
      File.AppendAllText(logFile, rval);
    }
    static long GetDirectorySize(string p)
    {
      // 1.
      // Get array of all file names.
      string[] a = Directory.GetFiles(p, "*.*");

      // 2.
      // Calculate total bytes of all files in a loop.
      long b = 0;
      foreach (string name in a)
      {
        // 3.
        // Use FileInfo to get length of each file.
        FileInfo info = new FileInfo(name);
        b += info.Length;
      }
      // 4.
      // Return total size
      return b;
    }
    static string FileSize(string fileName)
    {
      FileInfo fi = new FileInfo(fileName);
      var s = BytesToString(fi.Length);
      return s;
    }
    /// <summary>
    /// https://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
    /// </summary>
    /// <param name="byteCount"></param>
    /// <returns></returns>
    static String BytesToString(long byteCount)
    {
      string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
      if (byteCount == 0)
        return "0" + suf[0];
      long bytes = Math.Abs(byteCount);
      int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
      double num = Math.Round(bytes / Math.Pow(1024, place), 1);
      return (Math.Sign(byteCount) * num).ToString() + suf[place];
    }

  }
}
