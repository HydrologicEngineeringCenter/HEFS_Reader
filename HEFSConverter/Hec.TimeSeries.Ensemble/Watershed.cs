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
    }
    public string Name { get; set; }

    public List<Location> Locations { get; set; }
    
  }
}
