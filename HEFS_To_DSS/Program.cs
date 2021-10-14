using Hec.TimeSeries.Ensemble;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFS_To_DSS
{
  class Program
  {
    static void Main(string[] args) {

      // read from web.

      // import
      var re = new CsvEnsembleReader(@"c:\temp\hefs_cache");
      var t = new DateTime(2021, 10, 1, 12, 0, 0);
      var ws = re.Read("RussianNapa", t, t);
      DssEnsemble.WriteToTimeSeriesProfiles(@"C:\temp\"+ws.Name+".dss", ws);
      Console.WriteLine(ws);
    }
  }
}
