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
			string[] newlines = { "/r/n" };
			string[] rows = data.Split(newlines,StringSplitOptions.None);
			//split based on comma

			return false;
		}
	}
}
