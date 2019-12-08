using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hec.TimeSeries.Ensemble
{
  public class Location
  {
    public Location(string name)
    {
      this.Name = name;
      Forecasts = new List<Forecast>();
    }
    public string Name { get; set; }
    /// <summary>
    /// Parent Watershed
    /// </summary>
    public Watershed Watershed { get; set; } 
    
    /// <summary>
    /// List of forecasts 
    /// </summary>
    public List<Forecast> Forecasts { get; set; }

  }
}
