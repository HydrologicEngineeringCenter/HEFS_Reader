using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HEFS_Reader.Interfaces;

namespace HEFS_Reader.Implementations
{
	class HEFS_ZIP_Writer : IEnsembleWriter
	{
		public bool Write(ITimeSeriesOfEnsembleLocations timeSeriesOfEnsembleLocations, string directoryPath)
		{
			foreach (IWatershed watershed in timeSeriesOfEnsembleLocations.timeSeriesOfEnsembleLocations)//this could be parallel.
			{
				string fileName = watershed.WatershedName.ToString() + "_" + HEFS_CSV_Parser.StringifyDateTime(watershed.Locations.First().IssueDate) + ".csv";
				string fullPath = System.IO.Path.Combine(directoryPath, fileName);

				StringBuilder sb = new StringBuilder("GMT");
				int byteLoc = 0;
				byte[] buff = null; 
				using (System.IO.FileStream fs = new System.IO.FileStream(fullPath, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write))
				{

					using (System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionLevel.Fastest))
					{

						foreach (IEnsemble e in watershed.Locations)
						{
							Int32 memberCounter = 1;
							foreach (IEnsembleMember m in e.Members)
							{
								sb.Append("," + e.LocationName + "_" + memberCounter);
								memberCounter++;
							}
						}
						sb.AppendLine("");
						//buff = Encoding.ASCII.GetBytes(sb.ToString());
						//gz.Write(buff, byteLoc, buff.Length);
						Int32 counter = 0;
						foreach (DateTime t in watershed.Locations.First().Members.First().Times)
						{
							sb.Append(t.ToString());
							foreach (IEnsemble e in watershed.Locations)
							{
								foreach (IEnsembleMember m in e.Members)
								{
									sb.Append("," + m.Values[counter]);
								}
							}
							sb.AppendLine("");
							counter++;
						}
						// convert StringBuilder to ByteArray
						buff = Encoding.ASCII.GetBytes(sb.ToString());
						gz.Write(buff, byteLoc, buff.Length);
					}
				}
			}
			return true;
		}
	}
}
