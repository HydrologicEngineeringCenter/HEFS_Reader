using System;
using System.Collections.Generic;

namespace HEFS_Reader.Interfaces
{
	public interface IEnsemble
	{
		DateTime RefereceDate { get; }
		DateTime IssueDate { get; }
		string LocationName { get; }
		Enumerations.Timesteps Timestep { get; }
		IList<IEnsembleMember> Members { get; }
	}
}
