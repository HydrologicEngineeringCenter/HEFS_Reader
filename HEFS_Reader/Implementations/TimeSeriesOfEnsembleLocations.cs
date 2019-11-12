using System;
using System.Collections.Generic;
using System.Linq;
using HEFS_Reader.Enumerations;
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
      //foreach (var ws in _forecasts)
      //{
      //  List<IEnsemble> ensembleSubset = new List<IEnsemble>();
        
      //  // Group them by name...
      //  var groupedEnsembles = ws.Locations.GroupBy(e => e.LocationName);
      //  foreach (var group in groupedEnsembles)
      //  {
      //    // Sort the group, take the first N. We can also filter by start/end date if we want to.
      //    ensembleSubset.AddRange(group.OrderBy(a => a.IssueDate).Take(takeCount));
      //  }
       
      //  retn._forecasts.Add(new WatershedForecast(ensembleSubset, ws.WatershedName,ws.IssueDate));
      //}

      retn._forecasts.AddRange(Forecasts.Take(takeCount));
      return retn;
    }

    public void SortWatersheds()
    {
      _forecasts = Forecasts.OrderBy(ws => ws.Locations.FirstOrDefault().IssueDate).ToList();
    }

    public override bool Equals(object obj)
    {
      ITimeSeriesOfEnsembleLocations o = obj as ITimeSeriesOfEnsembleLocations;
      if (o == null) return false;
      if (this.Forecasts.Count != o.Forecasts.Count)
      {
        Console.WriteLine("timeSeriesOfEnsembleLocations.Count does not match");
        return false;
      }
      for (int i = 0; i < this.Forecasts.Count; i++)
      {
        if (!this.Forecasts[i].Equals(o.Forecasts[i]))
        {
          Console.WriteLine("timeSeriesOfEnsembleLocations[" + i + "] does not match");
          return false;
        }
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
    private int IndexOfIssueDate(DateTime dt)
    {

      if (_forecasts.Count == 0) return -1;
      int idx = 0;
      foreach (IWatershedForecast w in _forecasts)
      {
        if (w.IssueDate.Equals(dt)) return idx;
        idx++;
      }
      return -1;
    }

    public void AddEnsembleMember(IEnsembleMember em, int emidx, DateTime issueDate, string locationName, Watersheds watershedName)
    {
      int idx = IndexOfIssueDate(issueDate);
      if (idx == -1)
      {
        //need to sort based on issueDate
        _forecasts.Add(new WatershedForecast(new List<IEnsemble>(), watershedName, issueDate));
        idx = IndexOfIssueDate(issueDate);

      }
      _forecasts[idx].AddEnsembleMember(em, emidx, locationName);
    }
  }
}
