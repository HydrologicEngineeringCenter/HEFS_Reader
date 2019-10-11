using DSSIO;
using HEFS_Reader.Enumerations;
using HEFS_Reader.Implementations;
using HEFS_Reader.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

        class PathComparer : IComparer<DSSPath>
        {
            public int Compare(DSSPath x, DSSPath y)
            {

                return string.Compare(CollectionSortable(x),CollectionSortable(y));
            }

//            string previousT = dssPaths[0].Fpart.Split('|').Last().Split(':').Last();
  //          string previousLoc = dssPaths[0].Bpart;
            // /RUSSIANNAPA/APCC1/FLOW/01SEP2019/1HOUR/C:000002|T:0212019 

            private string CollectionSortable(DSSPath x)
            {
                string rval = x.Apart + x.Bpart + x.Cpart + x.SortableDPart + x.Epart;
                     
                var tokens = x.Fpart.Split('|');
                if (tokens.Length != 2)
                    return x.PathWithoutDate;
                rval += tokens[1].Split(':')[1] + tokens[0].Split(':')[0];
                return rval;
            }
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
				DSSIO.DSSPathCollection rawDssPaths = dss.GetCondensedPathNames(); // sorted
                var dssPaths = rawDssPaths.OrderBy(a => a, new PathComparer()).ToArray(); // sorted


                int size = dssPaths.Length;
                if (size == 0)
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
                         TrimLeadingBaggage(ts);
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

        /// <summary>
        /// Trim leading missing data, can be caused by DSS Block alignment with data
        /// (move to DSSIO ?)
        /// </summary>
        /// <param name="ts"></param>
        private void TrimLeadingBaggage(DSSTimeSeries ts)
        {
            var vals = ts.Values;
            int idxFirstGood = 0;
            int idxLastGood = vals.Length - 1;
            if (vals.Length == 0)
                return;
            for (int i = 0; i < vals.Length; i++)
            {
                if (Hec.Dss.DSS.ZIsMissingFloat(vals[i]) != 1)
                {
                    idxFirstGood = i;
                    break;
                }
            }

            for (int i = vals.Length - 1; i >= 0; i--)
            {
                if (Hec.Dss.DSS.ZIsMissingFloat(vals[i]) != 1)
                {
                    idxLastGood = i;
                    break;
                }
            }
            if (idxFirstGood != 0 || idxLastGood != vals.Length - 1)
            { // 
                int len = idxLastGood - idxFirstGood + 1;
                var f = new double[len];
                DateTime[] t = new DateTime[len];
                Array.Copy(vals, idxFirstGood, f, 0, len);
                Array.Copy(ts.Times, idxFirstGood, t, 0, len);
                ts.Values = f;
                ts.Times = t;

            }


        }
    }
}
