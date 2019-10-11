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
		private static DateTime ParseIssueDate(string input, bool tPart = false)
		{
            if (!tPart)
            {
                input = input.Split('|')[1].Split(':')[1];
            }
            
			int year = Convert.ToInt32(input.Substring(3));
			string sday = input.Substring(0, 3);
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
                rval += tokens[1].Split(':')[1] + tokens[0].Split(':')[1];
                return rval;
            }
        }

        public ITimeSeriesOfEnsembleLocations ReadDataset(Watersheds watershed, DateTime start, DateTime end, string dssPath)
		{
			System.Diagnostics.Stopwatch st = new Stopwatch();
			st.Start();
			IList<HEFS_Reader.Interfaces.IEnsemble> ensembles = new List<HEFS_Reader.Interfaces.IEnsemble>();
			IWatershedForecast watershedForecast = null;
			TimeSeriesOfEnsembleLocations rval = new TimeSeriesOfEnsembleLocations();
			//IList<IWatershedForecast> watershedForecasts = rval.timeSeriesOfEnsembleLocations;


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
					DateTime issueDate = ParseIssueDate(path.Fpart);

                    if (issueDate >= start && issueDate <= end
                      && path.Apart.ToLower() == watershed.ToString().ToLower())
                    {
                        var ts = dss.GetTimeSeries(path.FullPath);
                        TrimLeadingBaggage(ts);

                      //  bigguy.Add( ensemble, watershed,  issueDate)

                        List<float> memberValues = new List<float>();

                        memberValues.AddRange(Array.ConvertAll(ts.Values, item => (float)item));
                        ensembleValues.Add(memberValues);



                        if (!previousT.Equals(currentT) || i == 0)//relying on sorted T and B
                        {
							//issue time has changed, we need a new Watershed Forecast, or to identify an existing one.

							int idx = 0;//rval.IndexOfIssueDate(issueDate);
							int prevIdx = 0;//rval.IndexOfIssueDate(ParseIssueDate(previousT, true));
                            if (i != 0 && prevIdx == -1)
                            {
                                rval.timeSeriesOfEnsembleLocations.Add(watershedForecast);//add the completed one!
                            }

                            if (idx == -1)
                            {
                                //this is a new csv
                                ensembles = new List<IEnsemble>();
                                watershedForecast = new WatershedForecast(ensembles, watershed, issueDate);
                            }
                            else
                            {
                                //fetch the old one!
                                watershedForecast = rval.timeSeriesOfEnsembleLocations[idx];
                                ensembles = watershedForecast.Locations;
                            }
                            previousT = currentT;
                            previousLoc = currentLoc;
                        }
                        bool contains = false;
                        Ensemble e = null;
                        foreach(Ensemble ens in ensembles)
                        {
                            if(ens.LocationName== currentLoc)
                            {
                                //why?
                                contains = true;
                                e = ens;
                                break;
                            }

                        }
                        if (!contains)
                        {
                           e = new Ensemble(currentLoc, issueDate, ensembleValues, ts.Times.ToList());
                           ensembles.Add(e);
                        }
                        else
                        {
                            e.Members.Add(new EnsembleMember(memberValues.ToArray(), ts.Times));
                            //throw new Exception("found data at " + currentLoc + " for time " + issueDate.ToString() + " but we think it already exists.")
                        }

                    }

				}

			}
			rval.timeSeriesOfEnsembleLocations.Add(watershedForecast);
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
