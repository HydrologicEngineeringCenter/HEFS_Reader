using System;
using System.Collections.Generic;
using HEFS_Reader.Enumerations;
using HEFS_Reader.Interfaces;

namespace HEFS_Reader.Implementations
{
	public class WatershedForecast : IWatershedForecast
  {
    public Watersheds WatershedName { get; }
    public IList<IEnsemble> Locations { get; }
    public DateTime IssueDate { get; }

    public WatershedForecast(IList<IEnsemble> ensembles, Watersheds watershedName, DateTime issueDate)
		{
      Locations = ensembles;
      WatershedName = watershedName;
      IssueDate = issueDate;
    }
  }
}
