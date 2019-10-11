using System;
using System.Collections.Generic;
using System.Text;
using HEFS_Reader.Enumerations;
using HEFS_Reader.Interfaces;

namespace HEFS_Reader.Implementations
{
	public class HEFS_CSV_Parser
	{
		public static IWatershedForecast ParseCSVData(string data, DateTime issueDate, Enumerations.Watersheds watershedName)
		{
			//is this zipped or not zipped?
			//split based on new lines into rows for each element.
			char[] newlines = { '\n' };
			string[] rows = data.Split(newlines);
			
			//split based on comma
			string[] header = rows[0].Split(',');
			string currHeader = "";
			var locStarts = new List<int>();
			var headers = new List<string>();
			var ensembles = new List<IEnsemble>();

      //first data element in header is timezone.
      for (int i = 1; i < header.Length; i++)
			{
				if (!currHeader.Equals(header[i])){
					currHeader = header[i];
					headers.Add(currHeader);
					locStarts.Add(i);
				}
			}
			
      //second line is Blank,QINE,...QINE
			bool isFirstPass = true;
			List<List<List<float>>> FullTable = new List<List<List<float>>>();//location, Ensemble member, values - because 64bit allows me to be careless
			List<DateTime> times = new List<DateTime>();
			for (int j = 2; j < rows.Length; j++) {
                if (rows[j].Trim() == "")
                    continue;
				string[] values = rows[j].Split(',');
				DateTime dt = ParseDateTime(values[0]);
				times.Add(dt);
				int locationNum = -1;//
				
				int ensembleMemberNum = 0;
				for (int i = 1; i < values.Length; i++)
				{
					//these are the values for the date dt.
					if (locStarts.Contains(i))
					{
						if (isFirstPass)
						{
							//add a location list.
							FullTable.Add(new List<List<float>>());
						}
						locationNum++;
						ensembleMemberNum = 0;
					}
					if (isFirstPass)
					{
						//add an ensemble member list.
						FullTable[locationNum].Add(new List<float>());
					}
					FullTable[locationNum][ensembleMemberNum].Add(float.Parse(values[i]));
                    ensembleMemberNum++;
				}
				isFirstPass = false;
			}

			//push into ensembles.
			for (int i = 0; i < FullTable.Count; i++)
			{
				ensembles.Add(new Ensemble(headers[i], issueDate, FullTable[i], times));
			}

			return new WatershedForecast(ensembles,watershedName);
		}


        public static DateTime ParseDateTime(string dt)
		{
			string[] dateTime = dt.Split(' ');
			string[] yyyymmdd = dateTime[0].Split('-');
			string[] hhmmss = dateTime[1].Split(':');
			DateTime output = new DateTime(int.Parse(yyyymmdd[0]), int.Parse(yyyymmdd[1]), int.Parse(yyyymmdd[2]), int.Parse(hhmmss[0]), int.Parse(hhmmss[1]), int.Parse(hhmmss[2]));
			return output;
		}
		public static string StringifyDateTime(DateTime input)
		{
			string output = "";
			output = input.Year.ToString() + StringifyInt(input.Month) + StringifyInt(input.Day) + StringifyInt(input.Hour);
			return output;
		}
		public static string StringifyInt(int input)
		{
			if (input < 10) return "0" + input.ToString();
			return input.ToString();
		}
	}
}
