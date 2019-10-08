using System;
using System.Collections.Generic;
using System.Text;

namespace HEFS_Reader.Interfaces
{
	public interface IEnsembleMember
	{
		/// <summary>
		/// The times for each value in the ensemble member
		/// </summary>
		DateTime[] Times { get; }
		/// <summary>
		/// The values for each timestep in the ensemble member.
		/// </summary>
		float[] Values { get; }
		float ComparisonTolerance{ get; set; }
	}
}
