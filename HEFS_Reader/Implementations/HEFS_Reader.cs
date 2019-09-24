﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HEFS_Reader.Implementations
{
	class HEFS_Reader
	{
		public IList<Interfaces.IEnsemble> readData(string data, DateTime issueDate)
		{
			//is this zipped or not zipped?
			//split based on new lines into rows for each element.
			char[] newlines = { '\n' };
			string[] rows = data.Split(newlines);
			
			//split based on comma
			string[] header = rows[0].Split(',');
			List<int> locStarts = new List<int>();
			string currHeader = "";
			List<string> headers = new List<string>();
			List <Interfaces.IEnsemble> ensembles = new List<Interfaces.IEnsemble>();

			for (int i = 1; i < header.Length; i++)//first data element in header is timezone.
			{
				if (!currHeader.Equals(header[i])){
					currHeader = header[i];
					headers.Add(currHeader);
					locStarts.Add(i);
				}
			}
			//second line is Blank,QINE,...QINE
			//
			bool isFirstPass = true;
			List<List<List<float>>> FullTable = new List<List<List<float>>>();//location, Ensemble member, values - because 64bit allows me to be careless
			List<DateTime> times = new List<DateTime>();
			for (int j = 2; j < rows.Length; j++) {
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
				}
				isFirstPass = false;
			}
			//push into ensembles.
			for (int i = 0; i < FullTable.Count; i++)
			{
				ensembles.Add(new Ensemble(headers[i], issueDate, FullTable[i], times));
			}


			return ensembles;
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
