using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hec.TimeSeries.Ensemble
{
  public class Watershed
  {

    public Watershed(string name)
    {
      this.Name = name;
      Locations = new List<Location>(10);
    }
    public string Name { get; set; }

    public List<Location> Locations { get; set; }

    internal Forecast AddForecast(string locName, DateTime issueDate, float[,] ensemble)
    {
      int idx = Locations.FindIndex(x => x.Name.Equals(locName));
      Location loc = null;
      if (idx >= 0)
        loc = Locations[idx];
      else
      {
        loc = new Location(locName,this);
        Locations.Add(loc);
      }

      var rval= loc.AddForecast(issueDate,  ensemble);
      return rval;
    }
  }
}
