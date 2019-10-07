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
		/// <summary>
		/// This is where the magic of each writer should happen - write out a whole time series of ensembles for a watershed. Do it as fast and as compressed as you can. Winner gets a beer (or some equivalent prize) from Will. 
		/// </summary>
		/// <param name="timeSeriesOfEnsembleLocations">An in memory representation of all ensembles across a time range for all locations in a watershed.</param>
		/// <param name="directoryName">the output directory (define your own file name)</param>
		/// <returns>The time it took to write!</returns>
		TimeSpan Write(ITimeSeriesOfEnsembleLocations timeSeriesOfEnsembleLocations, string directoryName);
	}
}
