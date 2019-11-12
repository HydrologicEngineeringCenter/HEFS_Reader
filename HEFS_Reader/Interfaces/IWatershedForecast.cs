using System;
using System.Collections.Generic;

namespace HEFS_Reader.Interfaces
{
	public interface IWatershedForecast
	{
		/// <summary>
		/// Within a watershed there is an ensemble for each location in the watershed. 
    /// Each ensemble is made up of ensemble members, the order (and count) of the ensemble 
    /// members should be consisent across all locations. The ith ensemble member 
    /// represents one estimated forecasted state
		/// </summary>
		IList<IEnsemble> Locations { get; }

		/// <summary>
		/// The name of the watershed as defined by the cnrfc website (it is their spelling and capitalization, not mine.)
		/// </summary>
		Enumerations.Watersheds WatershedName { get; }

    DateTime IssueDate { get; }

    void AddEnsembleMember(IEnsembleMember em, int ensembleMemberIndex, string location);
  }
}