using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hec.TimeSeries.Ensemble
{
  public class Forecast
  {

    public Forecast(Watershed parent, Location location, DateTime issueDate, float[,] ensemble)
    {
      this.Watershed = parent;
      this.Location = location;
      this.IssueDate = issueDate;
      this.Ensemble = ensemble;
    }

    Watershed Watershed { get; set; }

    /// <summary>
    /// Location of this forecast
    /// </summary>
    public Location Location { get; set; }

    public DateTime IssueDate { get; set; }

    public List<DateTime> TimeStamps { get; set; }

    public float[,] Ensemble { get; set; }

    public float[]  EnsembleMember(int index)
    {
      float[] rval = new float[Ensemble.GetLength(0)];

      for (int i = 0; i < rval.Length; i++)
      {
        rval[i] = Ensemble[index, i];
      }
      return rval;
    }



  }
}
