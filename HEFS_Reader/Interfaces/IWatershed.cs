﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFS_Reader.Interfaces
{
	public interface IWatershed
	{
		/// <summary>
		/// Within a watershed there is an ensemble for each location in the watershed. Each ensemble is made up of ensemble members, the order (and count) of the ensemble members should be consisent across all locations. The ith ensemble member represents one estimated forecasted state
		/// </summary>
		IList<IEnsemble> Locations { get; }
		/// <summary>
		/// the name of the watershed as defined by the cnrfc website (it is their spelling and capitalization, not mine.)
		/// </summary>
		Enumerations.Watersheds WatershedName { get; }
	}
}