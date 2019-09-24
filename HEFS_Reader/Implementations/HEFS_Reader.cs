using System;
using System.Collections.Generic;
using System.Text;

namespace HEFS_Reader.Implementations
{
	class HEFS_Reader
	{
		public bool readData(string data, DateTime issueDate)
		{
			//is this zipped or not zipped?
			//split based on new lines into rows for each element.
			char[] newlines = { '\n' };
			string[] rows = data.Split(newlines);
			
			//split based on comma
			string[] header = rows[0].Split(',');
			List<int> locStarts = new List<int>();
			string currHeader = "";

			List <Interfaces.IEnsemble> ensembles = new List<Interfaces.IEnsemble>();

			for (int i = 1; i < header.Length; i++)//first data element in header is timezone.
			{
				if (!currHeader.Equals(header[i])){
					currHeader = header[i];
					ensembles.Add(new Ensemble(currHeader, issueDate));
					locStarts.Add(i);
				}
			}
			//second line is Blank,QINE,...QINE
			//
			for (int j = 2; j < rows.Length; j++) {
				string[] values = rows[j].Split(',');
				DateTime dt = ParseDateTime(values[0]);
				List<float> timesliceAcrossMembers = new List<float>();
				int locationNum = -1;//
				for (int i = 1; i < values.Length; i++)
				{
					//these are the values for the date dt.
					if (locStarts.Contains(i))
					{
						locationNum++;
						if (locationNum != 0) {
							ensembles[locationNum-1].AddSlice(dt, timesliceAcrossMembers);
							timesliceAcrossMembers = new List<float>();
						}
						
						timesliceAcrossMembers.Add(float.Parse(values[i]));
					}
				}
			}
			return false;
		}
		private DateTime ParseDateTime(string dt)
		{
			string[] dateTime = dt.Split(' ');
			string[] yyyymmdd = dateTime[0].Split('-');
			string[] hhmmss = dateTime[1].Split(':');
			DateTime output = new DateTime(int.Parse(yyyymmdd[0]), int.Parse(yyyymmdd[1]), int.Parse(yyyymmdd[2]), int.Parse(hhmmss[0]), int.Parse(hhmmss[1]), int.Parse(hhmmss[2]));
			return output;
		}
	}
}
