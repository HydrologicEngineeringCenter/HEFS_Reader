using Hec.TimeSeries.Ensemble;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pipe_cutter
{
  class Program
  {
    static void Main(string[] args)
    {

      Stopwatch swTotal = Stopwatch.StartNew();
      var fn = @"c:\temp\pisces-ensemble-test.pdb";
      File.Delete(fn);
      CsvEnsembleReader r = new CsvEnsembleReader(EnsembleTester.CacheDir);

      var watershedNames = new string[] { "RussianNapa", "EastSierra", "FeatherYuba" };
      //var watershedNames = new string[] {  "EastSierra", "FeatherYuba" };
      var sw = new Stopwatch();
      foreach (var name in watershedNames)
      {
        var t1 = new DateTime(2013, 11, 3, 12, 0, 0);
        var t2 = new DateTime(2018, 11, 3, 12, 0, 0);
        var ws = r.Read(name, t1, t2);

        sw.Start();
        bool piscesFormat = true;
        SqLiteEnsemble.Write(fn, ws, true, piscesFormat);
        sw.Stop();
      }

      Console.WriteLine(sw.Elapsed.TotalSeconds.ToString("F1")+" Pisces: elapsed seconds");
      Console.WriteLine(swTotal.Elapsed.TotalSeconds.ToString("F1")+" Total elapsed seconds");

    }
  }
}
