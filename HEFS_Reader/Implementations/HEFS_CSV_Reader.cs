using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using HEFS_Reader.Enumerations;
using HEFS_Reader.Interfaces;

namespace HEFS_Reader.Implementations
{
	public class HEFS_CSV_Reader : Interfaces.IEnsembleReader
	{
		//private const string _rootUrl = "https://www.cnrfc.noaa.gov/csv/";
    private long _watershedReadTime = 0;
    public long ReadTimeInMilliSeconds { get { return _watershedReadTime; } }


    public HEFS_CSV_Reader()
        {
             //_cacheDirectory = Path.GetTempPath();
        }
        public Interfaces.IWatershedForecast Read(Interfaces.IHEFSReadArgs args)
		{
            //https://www.cnrfc.noaa.gov/csv/2019092312_RussianNapa_hefs_csv_hourly.zip
            //string webrequest = _rootUrl;


             string fileName  = args.ForecastDate.ToString("yyyyMMddhh") + "_";
            fileName += args.WatershedLocation.ToString();
            fileName += "_hefs_csv_hourly";
      //webrequest += fileName+".zip";

      //System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            //string zipFileName = Path.Combine(CacheDirectory, fileName+".zip");
            string csvFileName = Path.Combine(args.Path, fileName + ".csv");
            if (File.Exists(csvFileName))
            {
                Console.WriteLine("Found "+ csvFileName+" in cache.  Reading...");
        //st.Start();
        Interfaces.IWatershedForecast w = HEFS_CSV_Parser.ParseCSVData(File.ReadAllText(csvFileName), 
           args.ForecastDate, args.WatershedLocation);
        //st.Stop();
        //_watershedReadTime = st.ElapsedMilliseconds;
        return w;
            }

			Console.WriteLine("Warning: "+csvFileName+" not found, skipping");
			return null;
           
		}

    public Interfaces.ITimeSeriesOfEnsembleLocations ReadDataset(Watersheds watershed, DateTime start, DateTime end, string Path)
    {
			System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
			st.Start();
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
      Interfaces.ITimeSeriesOfEnsembleLocations output = new TimeSeriesOfEnsembleLocations();
      DateTime endTimePlus1 = end.AddDays(1.0);
      while (!start.Equals(endTimePlus1))
      {
        Interfaces.IWatershedForecast wtshd = Read(args);
        if (wtshd != null)
        {
          output.timeSeriesOfEnsembleLocations.Add(wtshd);
        }
        else
        {
          //dont add null data?
        }
        start = start.AddDays(1.0);
        args.ForecastDate = start;

      }
			st.Stop();
			_watershedReadTime = st.ElapsedMilliseconds;
      return output;
    }
  }
}

