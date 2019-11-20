using System;
using System.Collections.Generic;
using System.Linq;
using HEFS_Reader.Enumerations;
using HEFS_Reader.Interfaces;

namespace HEFS_Reader.Implementations
{
  public class TimeSeriesOfEnsembleLocations 
  {
    private List<WatershedForecast> _forecasts;
    public List<WatershedForecast> Forecasts { get => _forecasts; }

    public TimeSeriesOfEnsembleLocations()
    {
      _forecasts = new List<WatershedForecast>();
    }

    public TimeSeriesOfEnsembleLocations CloneSubset(int takeCount)
    {
      var retn = new TimeSeriesOfEnsembleLocations();
      //foreach (var ws in _forecasts)
      //{
      //  List<Ensemble> ensembleSubset = new List<Ensemble>();
        
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
      TimeSeriesOfEnsembleLocations o = obj as TimeSeriesOfEnsembleLocations;
      if (o == null) return false;
      if (this.Forecasts.Count != o.Forecasts.Count)
      {
        Console.WriteLine("Forecasts.Count does not match.");
        Console.WriteLine("this.Forecasts.Count="+this.Forecasts.Count+" other Count ="+o.Forecasts.Count);
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
    public override int GetHashCode()
    {
      return 1866927581 + EqualityComparer<IList<WatershedForecast>>.Default.GetHashCode(Forecasts);
    }
    private int IndexOfIssueDate(DateTime dt)
    {

      if (_forecasts.Count == 0) return -1;
      int idx = 0;
      foreach (WatershedForecast w in _forecasts)
      {
        if (w.IssueDate.Equals(dt)) return idx;
        idx++;
      }
      return -1;
    }

    public void AddEnsembleMember(EnsembleMember em, int emidx, DateTime issueDate, string locationName, Watersheds watershedName)
    {
      int idx = IndexOfIssueDate(issueDate);
      if (idx == -1)
      {
        //need to sort based on issueDate
        _forecasts.Add(new WatershedForecast(new List<Ensemble>(), watershedName, issueDate));
        idx = IndexOfIssueDate(issueDate);

      }
      _forecasts[idx].AddEnsembleMember(em, emidx, locationName);
    }

    public void Add(TimeSeriesOfEnsembleLocations toel)
    {
      _forecasts.AddRange(toel.Forecasts);
    }

  }
}
