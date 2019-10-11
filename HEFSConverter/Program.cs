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
using HEFS_Reader.Interfaces;

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
        DateTime endTime = GetEndTime(numEnsembles);

        // READ CSV 
        var provider = new HEFS_Reader.Implementations.HEFS_CSV_Reader();
        HEFS_Reader.Interfaces.ITimeSeriesOfEnsembleLocations baseWaterShedData =
        provider.ReadDataset(Watersheds.RussianNapa, startTime, endTime, cacheDir);


        // WRITE

        WriteToMultipleFormats(baseWaterShedData, numEnsembles, startTime);

        // READ

        ReadFromMultipleFormats(numEnsembles, startTime, endTime, baseWaterShedData);

        numEnsembles *= 10;
      }
    }

    private static void ReadFromMultipleFormats(int numEnsembles, DateTime startTime, DateTime endTime, ITimeSeriesOfEnsembleLocations baseWaterShedData)
    {
      var fn = "ensemble_sqlite_blob_compressed" + numEnsembles + ".db";
      IEnsembleReader reader = new SqlBlobEnsembleReader();
      var watershed = reader.ReadDataset(Watersheds.RussianNapa, startTime, endTime, fn);
      LogInfo(fn, numEnsembles, ((HEFS_Reader.Interfaces.ITimeable)reader).ReadTimeInMilliSeconds / 1000);//potentially unsafe action.
      ErrorCheck(fn, baseWaterShedData, watershed);

      fn = "ensemble_sqlite_blob" + numEnsembles + ".db";
      reader = new SqlBlobEnsembleReader();
      watershed = reader.ReadDataset(Watersheds.RussianNapa, startTime, endTime, fn);
      LogInfo(fn, numEnsembles, ((HEFS_Reader.Interfaces.ITimeable)reader).ReadTimeInMilliSeconds / 1000);//potentially unsafe action.
      ErrorCheck(fn, baseWaterShedData, watershed);


      fn = "ensemble_V7" + numEnsembles + ".dss";
      reader = new DssEnsembleReader();
      watershed = reader.ReadDataset(Watersheds.RussianNapa, startTime, endTime, fn);
      LogInfo(fn, numEnsembles, ((HEFS_Reader.Interfaces.ITimeable)reader).ReadTimeInMilliSeconds / 1000);//potentially unsafe action.
      ErrorCheck(fn, baseWaterShedData, watershed);
    }

    private static void ErrorCheck(string fn, ITimeSeriesOfEnsembleLocations baseWaterShedData, ITimeSeriesOfEnsembleLocations watershed)
    {
      if (!baseWaterShedData.Equals(watershed))
      {
        string msg = "ERROR comparing read from write ";
        LogInfo(fn, 0, 0);
        Console.WriteLine(msg);
      }
    }

    private static void WriteToMultipleFormats(HEFS_Reader.Interfaces.ITimeSeriesOfEnsembleLocations waterShedData, int numEnsembles, DateTime startTime)
		{
			TimeSpan ts;
			DateTime endTime = GetEndTime(numEnsembles);
			//  startTime = new DateTime(2016, 11, 1, 12, 0, 0);
			// endTime = new DateTime(2017, 2, 28, 12, 0, 0);

			

			File.AppendAllText(logFile, "\n");
			File.AppendAllText(logFile, startTime.ToString() + " -->  " + endTime.ToString() + "\n");

			HEFS_Reader.Implementations.HEFS_CSV_Writer w = new HEFS_CSV_Writer();
			var dir = Path.Combine(Directory.GetCurrentDirectory(), "csv_out_" + numEnsembles);
			if (Directory.Exists(dir))
				Directory.Delete(dir, true);
			Directory.CreateDirectory(dir);
			ts = w.Write(waterShedData, dir);
			LogInfo(dir, numEnsembles, ts.TotalSeconds, true);

			var fn = "ensemble_V7" + numEnsembles + ".dss";
			ts = DssEnsembleWriter.Write(fn, waterShedData, true, 7);
			LogInfo(fn, numEnsembles, ts.TotalSeconds);

			fn = "ensemble_V6" + numEnsembles + ".dss";
			ts = DssEnsembleWriter.Write(fn, waterShedData, true, 6);
			LogInfo(fn, numEnsembles, ts.TotalSeconds);


			//fn = "ensemble_sqlite" + numEnsembles + ".db";
			//File.Delete(fn);
			//var connectionString = "Data Source=" + fn + ";Synchronous=Off;Pooling=True;Journal Mode=Off";
			//Reclamation.Core.SQLiteServer server = new Reclamation.Core.SQLiteServer(connectionString);
			////ts = SqlEnsembleWriter.WriteToDatabase(server, startTime, endTime, true, cacheDir);
			//ts = SqlEnsembleWriter.Write(server, waterShedData);
			//server.CloseAllConnections();
			//LogInfo(fn, numEnsembles, ts.TotalSeconds);

			fn = "ensemble_sqlite_blob" + numEnsembles + ".db";
			File.Delete(fn);
			var connectionString = "Data Source=" + fn + ";Synchronous=Off;Pooling=True;Journal Mode=Off";
			var server = new Reclamation.Core.SQLiteServer(connectionString);
			server.CloseAllConnections();
			ts = SqlBlobEnsembleWriter.Write(server, waterShedData, false);
			LogInfo(fn, numEnsembles, ts.TotalSeconds);

			fn = "ensemble_sqlite_blob_compressed" + numEnsembles + ".db";
			File.Delete(fn);
			connectionString = "Data Source=" + fn + ";Synchronous=Off;Pooling=True;Journal Mode=Off";
			server = new Reclamation.Core.SQLiteServer(connectionString);
			server.CloseAllConnections();
			ts = SqlBlobEnsembleWriter.Write(server, waterShedData, true);
			LogInfo(fn, numEnsembles, ts.TotalSeconds);

			//  CreateSqLiteBlobDatabaseOfEnsembles("sqlite_blob_ensemble_float.pdb", startTime, endTime);
		}

		static void LogInfo(string path, int numEnsemblesToWrite, double seconds, bool isDir = false, string msg="")
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
			double mb = size / 1024.0 / 1024.0;
			double mbs = mb / seconds;

			string rval = path + ", " + numEnsemblesToWrite + ", " 
        + seconds.ToString("F2") + " s ," + BytesToString(size) + ", " + mbs.ToString("F2") + " mb/seconds"+msg+"\n";
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
