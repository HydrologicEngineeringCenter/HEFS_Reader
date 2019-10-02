using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HEFS_Reader.Interfaces;

namespace HEFS_Reader.Implementations
{
	class HEFS_CSV_Writer : Interfaces.IEnsembleWriter
	{
		public bool Write(ITimeSeriesOfEnsembleLocations timeSeriesOfEnsembleLocations, string directoryPath)
		{
			foreach (IWatershed watershed in timeSeriesOfEnsembleLocations.timeSeriesOfEnsembleLocations)//this could be parallel.
			{
				string fileName = watershed.WatershedName.ToString() + "_" + HEFS_CSV_Parser.StringifyDateTime(watershed.Locations.First().IssueDate) + ".csv";
				string fullPath = System.IO.Path.Combine(directoryPath, fileName);
				//System.IO.FileStream fs = new System.IO.FileStream(fullPath, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write);
				string line = "";//consider stringbuilder.
				using (System.IO.StreamWriter sr = new System.IO.StreamWriter(fullPath))
				{
					line = "GMT";
					foreach (IEnsemble e in watershed.Locations)
					{
						Int32 memberCounter = 1;
						foreach (IEnsembleMember m in e.Members)
						{
							line += "," + e.LocationName + "_" + memberCounter;
							memberCounter++;
						}
					}
					sr.WriteLine(line);
					Int32 counter = 0;
					foreach (DateTime t in watershed.Locations.First().Members.First().Times)
					{
						line = t.ToString();
						foreach (IEnsemble e in watershed.Locations)
						{
							foreach (IEnsembleMember m in e.Members)
							{
								line += "," + m.Values[counter];
							}
						}
						sr.WriteLine(line);
						counter++;
					}
				}
			}
			return true;
		}
	}
}
