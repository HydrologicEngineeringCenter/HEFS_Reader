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
		private IList<IWatershedForecast> _timeSeriesofEnsembleLocations;
		public IList<IWatershedForecast> timeSeriesOfEnsembleLocations
		{
			get
			{
				return _timeSeriesofEnsembleLocations;
			}
		}
		public TimeSeriesOfEnsembleLocations()
		{
			_timeSeriesofEnsembleLocations = new List<IWatershedForecast>();
		}
		public override bool Equals(object obj)
		{
			ITimeSeriesOfEnsembleLocations o = obj as ITimeSeriesOfEnsembleLocations;
			if (o == null) return false;
			if (this.timeSeriesOfEnsembleLocations.Count != o.timeSeriesOfEnsembleLocations.Count) return false;
			for (int i = 0; i < this.timeSeriesOfEnsembleLocations.Count; i++)
			{
				if (!this.timeSeriesOfEnsembleLocations[i].Equals(o.timeSeriesOfEnsembleLocations[i]))return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return 1866927581 + EqualityComparer<IList<IWatershedForecast>>.Default.GetHashCode(timeSeriesOfEnsembleLocations);
		}
	}
}
