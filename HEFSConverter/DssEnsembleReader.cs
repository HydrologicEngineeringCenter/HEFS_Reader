using HEFS_Reader.Enumerations;
using HEFS_Reader.Implementations;
using HEFS_Reader.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFSConverter
{
	class DssEnsembleReader : HEFS_Reader.Interfaces.IEnsembleReader, HEFS_Reader.Interfaces.ITimeable
	{
		private long _readTimeInMilliSeconds = 0;
		public long ReadTimeInMilliSeconds { get { return _readTimeInMilliSeconds; } }


		/// <summary>
		/// parse issue date from part F:
		/// C:000002|T:0212019
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private static DateTime ParseIssueDate(string fPart)
		{
			string[] tokens = fPart.Split('|')[1].Split(':');
			int year = Convert.ToInt32(tokens[1].Substring(3));
			string sday = tokens[1].Substring(0, 3);
			int day = Convert.ToInt32(sday);
			DateTime issueDate = new DateTime(year, 1, 1).AddDays(day - 1).AddHours(12);
			return issueDate;
		}

		public IWatershedForecast Read(IHEFSReadArgs args)
		{
			throw new NotImplementedException();
		}

		public ITimeSeriesOfEnsembleLocations ReadDataset(Watersheds watershed, DateTime start, DateTime end, string dssPath)
		{
			System.Diagnostics.Stopwatch st = new Stopwatch();
			st.Start();
			List<HEFS_Reader.Interfaces.IEnsemble> ensembles = new List<HEFS_Reader.Interfaces.IEnsemble>();
			WatershedForecast watershedForecast = new WatershedForecast(ensembles, watershed);
			TimeSeriesOfEnsembleLocations rval = new TimeSeriesOfEnsembleLocations();
			IList<IWatershedForecast> watershedForecasts = rval.timeSeriesOfEnsembleLocations;


			using (DSSIO.DSSReader dss = new DSSIO.DSSReader(dssPath))
			{
				DSSIO.DSSPathCollection dssPaths = dss.GetCondensedPathNames(); // sorted
				if (dssPaths.Count == 0)
				{
					throw new Exception("Empty DSS catalog");
				}
				//need all locations => b being *
				//List<String> locations = dssPaths.GetUniqueBParts();
				//need all collection members who end in T: args.ForecastDate.DayOfYear + args.ForecastDate.Year
				string previousT = dssPaths[0].Fpart.Split('|').Last().Split(':').Last();
				string previousLoc = dssPaths[0].Bpart;
				// /RUSSIANNAPA/APCC1/FLOW/01SEP2019/1HOUR/C:000002|T:0212019/
				List<List<float>> ensembleValues = new List<List<float>>();
				int size = dssPaths.Count;

				for (int i = 0; i < size; i++)
				{
					DSSIO.DSSPath path = dssPaths[i];
					string currentLoc = path.Bpart;
					string currentT = path.Fpart.Split('|').Last().Split(':').Last();//probably slowish operation
					if (!previousT.Equals(currentT))//relying on sorted T and B
					{
						//issue time has changed, we have read a whole csv.
						watershedForecasts.Add(watershedForecast);
						ensembles = new List<HEFS_Reader.Interfaces.IEnsemble>();
						watershedForecast = new WatershedForecast(ensembles, watershed);
						previousT = currentT;
						previousLoc = currentLoc;
					}

					DateTime issueDate = ParseIssueDate(path.Fpart);
					if (issueDate >= start && issueDate <= end
					  && path.Apart.ToLower() == watershed.ToString().ToLower())
					{
						var ts = dss.GetTimeSeries(path.FullPath);
						List<float> memberValues = new List<float>();
						memberValues.AddRange(Array.ConvertAll(ts.Values, item => (float)item));
						ensembleValues.Add(memberValues);
						if (i == size - 1 || dssPaths[i + 1].Bpart != currentLoc)
						{// package this ensemble
							Ensemble e = new Ensemble(currentLoc, issueDate, ensembleValues, ts.Times.ToList());
							ensembles.Add(e);
							// start building next ensemble
							ensembleValues = new List<List<float>>();
						}
					}
				}

			}
			watershedForecasts.Add(watershedForecast);
			st.Stop();
			_readTimeInMilliSeconds = st.ElapsedMilliseconds;
			return rval;
		}
	}
}
