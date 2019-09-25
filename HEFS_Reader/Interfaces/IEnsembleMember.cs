using System;
using System.Collections.Generic;
using System.Text;

namespace HEFS_Reader.Interfaces
{
	public interface IEnsembleMember
	{
		DateTime[] getTimes();
		float[] getValues();
	}
}
