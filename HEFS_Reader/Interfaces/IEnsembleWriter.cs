using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFS_Reader.Interfaces
{
	public interface IEnsembleWriter
	{
		//bool Write(IEnsemble ensemble);
		bool Write(ITimeSeriesOfEnsembleLocations timeSeriesOfEnsembleLocations, string directoryName);
	}
}
