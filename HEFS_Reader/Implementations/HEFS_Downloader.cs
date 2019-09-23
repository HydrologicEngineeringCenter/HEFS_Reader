using System;
using System.Collections.Generic;
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
			//System.Net.WebClient rootwc = new System.Net.WebClient();
			string webrequest = _rootUrl;
			webrequest += args.date;
			webrequest += args.location;
			webrequest += "_hefs_csv_daily.zip";

			//System.Net.WebClient wc = new System.Net.WebClient();
			//Response = wc.DownloadString(webrequest);
			//search for error codes in the output.
			//Result = new ECAMResult(Response);

			return true;
			//new webconnection.

			//_Result = new ECAMResult(response);
		}
	}
}
//https://www.cnrfc.noaa.gov/ensembleHourlyProductCSV.php 
//var filetoget = '/csv/'+yyyy+monnum+daynew+hh+'_'+theprod+'_hefs_csv_hourly.zip'
//var printfiletoget = yyyy + monnum + daynew + hh + '_' + theprod + '_hefs_csv_hourly.zip
//https://www.cnrfc.noaa.gov/csv/2019092312_RussianNapa_hefs_csv_daily.zip