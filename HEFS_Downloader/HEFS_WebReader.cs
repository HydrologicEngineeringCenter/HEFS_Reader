using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using HEFS_Reader.Enumerations;
using HEFS_Reader.Interfaces;

namespace HEFS_Reader.Implementations
{
    public class HEFS_WebReader : Interfaces.IEnsembleReader
    {
        private const string _rootUrl = "https://www.cnrfc.noaa.gov/csv/";
        private long _watershedReadTime = 0;
        private string _cacheDirectory;
        public long ReadTimeInMilliSeconds { get { return _watershedReadTime; } }


        public HEFS_WebReader()
        {
            _cacheDirectory = Path.GetTempPath();
        }
        public Interfaces.IWatershedForecast Read(Interfaces.IHEFSReadArgs args)
        {
            //https://www.cnrfc.noaa.gov/csv/2019092312_RussianNapa_hefs_csv_hourly.zip
            string webrequest = _rootUrl;


            string fileName = args.ForecastDate.ToString("yyyyMMddhh") + "_";
            fileName += args.WatershedLocation.ToString();
            fileName += "_hefs_csv_hourly";
            webrequest += fileName + ".zip";

            string zipFileName = Path.Combine(args.Path, fileName + ".zip");
            string csvFileName = Path.Combine(args.Path, fileName + ".csv");
            if (File.Exists(csvFileName))
            {
                Console.WriteLine("Found " + csvFileName + " in cache.  skipping...");
                Interfaces.IWatershedForecast w = HEFS_CSV_Parser.ParseCSVData(File.ReadAllText(csvFileName),
                   args.ForecastDate, args.WatershedLocation);
                return w;
            }
            else
            { // download
                Console.WriteLine("downloading "+zipFileName);
                try
                {
                    GetFile(webrequest, zipFileName);
                }
                catch (System.Net.WebException ex)
                {
                    Console.WriteLine("404, "+zipFileName +" not found ");
                    return null;
                }
                Reclamation.Core.ZipFileUtility.UnzipFile(zipFileName, csvFileName);
                Interfaces.IWatershedForecast w = HEFS_CSV_Parser.ParseCSVData(File.ReadAllText(csvFileName),
                   args.ForecastDate, args.WatershedLocation);
                return w;
            }
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

        /// <summary>
        /// from https://stackoverflow.com/questions/137285/what-is-the-best-way-to-read-getresponsestream
        /// </summary>
        static void GetFile(string url, string outputFilename)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "GET";

            // if the URI doesn't exist, an exception will be thrown here...
            using (HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse())
            {
                using (Stream responseStream = httpResponse.GetResponseStream())
                {
                    using (FileStream localFileStream =
                        new FileStream(outputFilename, FileMode.Create))
                    {
                        var buffer = new byte[4096];
                        long totalBytesRead = 0;
                        int bytesRead;

                        while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            totalBytesRead += bytesRead;
                            localFileStream.Write(buffer, 0, bytesRead);
                        }
                    }
                }
            }
        }
    }
}