using System.Collections.Generic;

namespace HEFS_Reader.Interfaces
{
	public interface ITimeSeriesOfEnsembleLocations
	{
		/// <summary>
		/// This is a list of watershed objects, a watershed represents an entire watershed set of ensembles (one for each location in the watershed), each watershed in the list represents an ensemble forecast time.
		/// </summary>
		IList<IWatershedForecast> Forecasts { get; }

    ITimeSeriesOfEnsembleLocations CloneSubset(int takeCount);
	}
}
