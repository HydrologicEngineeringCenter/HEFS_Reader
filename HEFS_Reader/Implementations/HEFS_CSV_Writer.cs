using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HEFS_Reader.Interfaces;

namespace HEFS_Reader.Implementations
{
	public class HEFS_CSV_Writer : Interfaces.IEnsembleWriter
	{
		public TimeSpan Write(ITimeSeriesOfEnsembleLocations timeSeriesOfEnsembleLocations, string directoryPath)
		{
      System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
      st.Start();
			foreach (IWatershedForecast watershed in timeSeriesOfEnsembleLocations.timeSeriesOfEnsembleLocations)//this could be parallel.
			{
				string fileName = watershed.WatershedName.ToString() + "_" + HEFS_CSV_Parser.StringifyDateTime(watershed.Locations.First().IssueDate) + ".csv";
				string fullPath = System.IO.Path.Combine(directoryPath, fileName);
				StringBuilder line = new StringBuilder("GMT");
				using (System.IO.StreamWriter sr = new System.IO.StreamWriter(fullPath))
				{
					foreach (IEnsemble e in watershed.Locations)
					{
						Int32 memberCounter = 1;
						foreach (IEnsembleMember m in e.Members)
						{
							line.Append("," + e.LocationName + "_" + memberCounter);
							memberCounter++;
						}
					}
					sr.WriteLine(line.ToString());
					line = new StringBuilder();
					Int32 counter = 0;
					foreach (DateTime t in watershed.Locations.First().Members.First().Times)
					{
						line = new StringBuilder(t.ToString());
						foreach (IEnsemble e in watershed.Locations)
						{
							foreach (IEnsembleMember m in e.Members)
							{
								line.Append("," + m.Values[counter]);
							}
						}
						sr.WriteLine(line.ToString());
						counter++;
					}
				}
			}
      st.Stop();
			return st.Elapsed;
		}
	}
}
