using System;
using System.Collections.Generic;

namespace HEFS_Reader.Interfaces
{
	public interface IEnsemble
	{
		DateTime RefereceDate { get; }
		/// <summary>
		/// The date of issuance for the ensemble
		/// </summary>
		DateTime IssueDate { get; }
		/// <summary>
		/// The name of the location for the ensemble (there are many locations in a watershed typically)
		/// </summary>
		string LocationName { get; }
		/// <summary>
		/// The timestep of the ensemble members - right now i think this is not properly being set
		/// </summary>
		Enumerations.Timesteps Timestep { get; }
		/// <summary>
		/// The individual traces that make up an ensemble.
		/// </summary>
		IList<IEnsembleMember> Members { get; }

		void AddEnsembleMember(IEnsembleMember em, int ensembleMemberIndex);
	}
}
