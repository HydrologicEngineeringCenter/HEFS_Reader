using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hec.TimeSeries.Ensemble
{
  public class Forecast
  {

    public Forecast(string watershedName, string locationName, DateTime issueDate)
    {

    }
    /// <summary>
    /// Location of this forecast
    /// </summary>
    public Location Location { get; set; }

    public DateTime IssueDate { get; set; }
    
    public DateTime[] DateTime { get; set; }

    public float[][] Ensemble { get; set; }



  }
}
