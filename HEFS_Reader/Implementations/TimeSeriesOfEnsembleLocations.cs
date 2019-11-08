using System;
using System.Collections.Generic;
using System.Linq;
using HEFS_Reader.Interfaces;

namespace HEFS_Reader.Implementations
{
  public class TimeSeriesOfEnsembleLocations : ITimeSeriesOfEnsembleLocations
  {
    private List<IWatershedForecast> _forecasts;
    public IList<IWatershedForecast> Forecasts { get => _forecasts; }

    public TimeSeriesOfEnsembleLocations()
    {
      _forecasts = new List<IWatershedForecast>();
    }

    public ITimeSeriesOfEnsembleLocations CloneSubset(int takeCount)
    {
      var retn = new TimeSeriesOfEnsembleLocations();
      //foreach(var ws in Watersheds)
      //{
      //  List<IEnsemble> ensembleSubset = new List<IEnsemble>();

      //  // Group them by name...
      //  var groupedEnsembles = ws.Locations.GroupBy(e => e.LocationName);
      //  foreach(var group in groupedEnsembles)
      //  {
      //    // Sort the group, take the first N. We can also filter by start/end date if we want to.
      //    ensembleSubset.AddRange(group.OrderBy(a => a.IssueDate).Take(takeCount));
      //  }

      //  retn.Watersheds.Add(new WatershedForecast(ensembleSubset, ws.WatershedName));
      //}

      retn._forecasts.AddRange(Forecasts.Take(takeCount));
      return retn;
    }

    public void SortWatersheds()
    {
      _forecasts = Forecasts.OrderBy(ws => ws.Locations.FirstOrDefault().IssueDate).ToList();
    }
    public void AddEnsembleMember(IEnsembleMember em, int ensembleMemberIndex, DateTime issueDate, string location, Enumerations.Watersheds watershedName)
    {
      // Copied from master to get it to compile.
      // It doesn't have an implementation in master, so just add it to a list to make sure it's working
      _randomEnsembles.Add(em);
    }

    private List<IEnsembleMember> _randomEnsembles = new List<IEnsembleMember>();

    public override bool Equals(object obj)
    {
      ITimeSeriesOfEnsembleLocations o = obj as ITimeSeriesOfEnsembleLocations;
      if (o == null) return false;
      if (this.Forecasts.Count != o.Forecasts.Count) return false;
      for (int i = 0; i < this.Forecasts.Count; i++)
      {
        if (!this.Forecasts[i].Equals(o.Forecasts[i])) return false;
      }
      return true;
    }

    public void SortByIssuanceDate()
    {
      // copied from master
      _forecasts.Sort();
    }
    public override int GetHashCode()
    {
      return 1866927581 + EqualityComparer<IList<IWatershedForecast>>.Default.GetHashCode(Forecasts);
    }

  }
}
