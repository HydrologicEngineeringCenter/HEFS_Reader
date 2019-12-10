using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hec.TimeSeries.Ensemble
{
  public class Location
  {
    public Location(string name, Watershed watershed =null)
    {
      this.Name = name;
      Forecasts = new List<Forecast>();
      this.Watershed = watershed;
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

    internal Forecast AddForecast(DateTime issueDate, float[,] ensemble)
    {
      Forecast f = new Forecast(this.Watershed, this, issueDate,ensemble);
      Forecasts.Add(f);
      return f;
    }
  }
}
