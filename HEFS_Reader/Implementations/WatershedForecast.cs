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
    public override bool Equals(object obj)
    {
      WatershedForecast o = (WatershedForecast)obj;
      if (o == null)
      {
        return false;
      }
      if (this.Locations.Count != o.Locations.Count)
      {
        Console.WriteLine(this.ToString() + " Locations.Count does not match");
        Console.WriteLine("this has " + this.Locations.Count + " , and the other has " + o.Locations.Count);
        return false;
      }
      foreach (Ensemble e in this.Locations)
      {
        for (int i = 0; i < o.Locations.Count; i++)
        {
          if (e.LocationName.Equals(o.Locations[i].LocationName))
          {
            if (!e.Equals(o.Locations[i]))
            {
              Console.WriteLine("at o.Locations[" + i + "]." + o.Locations[i].LocationName + " ensemble does not match");
              return false;
            }
          }
        }
      }
      return true;
    }

    public override int GetHashCode()
    {
      var hashCode = -1316565584;
      hashCode = hashCode * -1521134295 + EqualityComparer<IList<Ensemble>>.Default.GetHashCode(Locations);
      hashCode = hashCode * -1521134295 + WatershedName.GetHashCode();
      return hashCode;

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
