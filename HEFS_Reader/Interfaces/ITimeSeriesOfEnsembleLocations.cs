using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFS_Reader.Interfaces
{
	public interface ITimeSeriesOfEnsembleLocations
	{
		/// <summary>
		/// This is a list of watershed objects, a watershed represents an entire watershed set of ensembles (one for each location in the watershed), each watershed in the list represents an ensemble forecast time.
		/// </summary>
		IList<IWatershedForecast> timeSeriesOfEnsembleLocations { get; }
		
	}
}
