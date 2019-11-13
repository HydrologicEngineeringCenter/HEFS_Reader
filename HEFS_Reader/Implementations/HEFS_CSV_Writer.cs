using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using HEFS_Reader.Interfaces;

namespace HEFS_Reader.Implementations
{
	public class HEFS_CSV_Writer : Interfaces.IEnsembleWriter
	{
		public void Write(TimeSeriesOfEnsembleLocations timeSeriesOfEnsembleLocations, string directoryPath)
		{

			foreach (WatershedForecast watershed in timeSeriesOfEnsembleLocations.Forecasts)//this could be parallel.
			{
				string fileName = watershed.WatershedName.ToString() + "_" + HEFS_CSV_Parser.StringifyDateTime(watershed.Locations.First().IssueDate) + ".csv";
				string fullPath = System.IO.Path.Combine(directoryPath, fileName);
				StringBuilder line = new StringBuilder("GMT");
				using (System.IO.StreamWriter sr = new System.IO.StreamWriter(fullPath))
				{
					foreach (Ensemble e in watershed.Locations)
					{
						Int32 memberCounter = 1;
						foreach (EnsembleMember m in e.Members)
						{
							line.Append("," + e.LocationName + "_" + memberCounter);
							memberCounter++;
						}
					}
					sr.WriteLine(line.ToString());
					line = new StringBuilder();

					int counter = 0;
					foreach (DateTime t in watershed.Locations.First().Members.First().Times)
					{
						line = new StringBuilder(t.ToString());
						foreach (Ensemble e in watershed.Locations)
						{
							foreach (EnsembleMember m in e.Members)
							{
								line.Append("," + m.Values[counter]);
							}
						}
						sr.WriteLine(line.ToString());
						counter++;
					}
				}
			}
		}
	}
}
