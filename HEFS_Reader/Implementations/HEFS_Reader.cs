using System;
using System.Collections.Generic;
using System.Text;

namespace HEFS_Reader.Implementations
{
	class HEFS_Reader
	{
		public bool readData(string data)
		{
			//is this zipped or not zipped?
			//split based on new lines into rows for each element.
			char[] newlines = { '\n' };
			string[] rows = data.Split(newlines);
			List<string> locationNames = new List<string>();
			
			//split based on comma
			string[] header = rows[0].Split(',');
			List<int> locStarts = new List<int>();
			string currHeader = "";
			for (int i = 1; i < header.Length; i++)//first data element in header is timezone.
			{
				if (!currHeader.Equals(header[i])){
					currHeader = header[i];
					locationNames.Add(currHeader);
					locStarts.Add(i);
				}
			}
			//second line is Blank,QINE,...QINE
			//

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
