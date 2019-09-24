﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace HEFS_Reader.Implementations
{
	class HEFS_Downloader
	{
		private string _rootUrl = "https://www.cnrfc.noaa.gov/csv/";
		private IList<Interfaces.IEnsemble> _Result;//if i make an abstract result, errors could be stored in the result property.
		public IList<Interfaces.IEnsemble> Result
		{
			get { return _Result; }
			set { _Result = value; }
		}
		public string Response { get; set; }
		public bool FetchData(HEFSRequestArgs args)
		{
			string webrequest = _rootUrl;
			webrequest += args.date;
			webrequest += args.location;
			webrequest += "_hefs_csv_hourly.zip";

            string zipFileName = Path.GetTempFileName();
            string csvFileName = Path.GetTempFileName();

            File.Delete(zipFileName);
            File.Delete(csvFileName);
            GetFile(webrequest, zipFileName);

            Reclamation.Core.ZipFileUtility.UnzipFile(zipFileName, csvFileName);
            Response = File.ReadAllText(csvFileName);
			return true;
			//new webconnection.

			//_Result = new ECAMResult(response);
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