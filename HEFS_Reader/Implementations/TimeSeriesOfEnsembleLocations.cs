using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HEFS_Reader.Interfaces;

namespace HEFS_Reader.Implementations
{
	public class TimeSeriesOfEnsembleLocations : Interfaces.ITimeSeriesOfEnsembleLocations
	{
		private IList<IWatershed> _timeSeriesofEnsembleLocations;
		public IList<IWatershed> timeSeriesOfEnsembleLocations
		{
			get
			{
				return _timeSeriesofEnsembleLocations;
			}
		}
		public TimeSeriesOfEnsembleLocations()
		{
			_timeSeriesofEnsembleLocations = new List<IWatershed>();
		}
	}
}
