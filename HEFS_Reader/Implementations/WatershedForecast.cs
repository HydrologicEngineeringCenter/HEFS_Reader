using System;
using System.Collections.Generic;
using HEFS_Reader.Enumerations;
using HEFS_Reader.Interfaces;

namespace HEFS_Reader.Implementations
{
	public class WatershedForecast
  {
    public Watersheds WatershedName { get; }
    public IList<Ensemble> Locations { get; }
    public DateTime IssueDate { get; }

    public WatershedForecast(IList<Ensemble> ensembles, Watersheds watershedName, DateTime issueDate)
		{
      Locations = ensembles;
      WatershedName = watershedName;
      IssueDate = issueDate;
    }

    public void AddEnsembleMember(EnsembleMember em, int ensembleMemberIndex, string location)
    {
      Ensemble ensembleAtLocation = null;
      foreach (Ensemble e in Locations)
      {
        if (e.LocationName.Equals(location))
        {
          ensembleAtLocation = e;
          break;
        }
      }

      if (ensembleAtLocation == null)
      {
        ensembleAtLocation = new Ensemble(location, IssueDate, new List<List<float>>(), em.Times);
        Locations.Add(ensembleAtLocation);
      }

      ensembleAtLocation.AddEnsembleMember(em, ensembleMemberIndex);
    }
  }
}
