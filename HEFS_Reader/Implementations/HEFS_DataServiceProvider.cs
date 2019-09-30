using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace HEFS_Reader.Implementations
{
	public class HEFS_DataServiceProvider : Interfaces.IHEFS_DataServiceProvider
	{
		private const string _rootUrl = "https://www.cnrfc.noaa.gov/csv/";
        private readonly string _cacheDirectory;

        public string Response { get; set; }

        public string CacheDirectory
        {
            get { return _cacheDirectory; }
            //private set { _cacheDirectory = value; }
        }
        public HEFS_DataServiceProvider()
        {
             _cacheDirectory = Path.GetTempPath();
        }
        public HEFS_DataServiceProvider(string cacheDir)
        {
            _cacheDirectory = cacheDir;
        }
        public bool FetchData(HEFSRequestArgs args)
		{
            //https://www.cnrfc.noaa.gov/csv/2019092312_RussianNapa_hefs_csv_hourly.zip
            string webrequest = _rootUrl;


             string fileName  = args.date + "_";
            fileName += args.location.ToString();
            fileName += "_hefs_csv_hourly";
            webrequest += fileName+".zip";

            
            string zipFileName = Path.Combine(CacheDirectory, fileName+".zip");
            string csvFileName = Path.Combine(CacheDirectory, fileName + ".csv");
            if (File.Exists(csvFileName))
            {
                Console.WriteLine("Found "+ csvFileName+" in cache.  Reading...");
                Response = File.ReadAllText(csvFileName);
                return true;
            }
           
                Console.WriteLine("GET "+webrequest);
            File.Delete(zipFileName);
            File.Delete(csvFileName);
            try
            {
                GetFile(webrequest, zipFileName);
            }
            catch( Exception exception)
            {
                Console.WriteLine("download failed");
                File.Delete(zipFileName);
                File.Delete(csvFileName);
                return false;
            }
            Reclamation.Core.ZipFileUtility.UnzipFile(zipFileName, csvFileName);
            Response = File.ReadAllText(csvFileName);
            Console.WriteLine("sucessfully downloaded to "+csvFileName);


            return true;
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
//https://www.cnrfc.noaa.gov/ensembleHourlyProductCSV.php 
//var filetoget = '/csv/'+yyyy+monnum+daynew+hh+'_'+theprod+'_hefs_csv_hourly.zip'
//var printfiletoget = yyyy + monnum + daynew + hh + '_' + theprod + '_hefs_csv_hourly.zip
//https://www.cnrfc.noaa.gov/csv/2019092312_RussianNapa_hefs_csv_hourly.zip