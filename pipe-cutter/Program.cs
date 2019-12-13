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
      bool piscesFormat = false;
      Stopwatch swTotal = Stopwatch.StartNew();
      var fn = @"c:\temp\sqlite-ensemble-test.db";
      if( piscesFormat)
         fn = @"c:\temp\pisces-ensemble-test.pdb";


      File.Delete(fn);
      CsvEnsembleReader r = new CsvEnsembleReader(EnsembleTester.CacheDir);

      var watershedNames = new string[] { "RussianNapa", "EastSierra", "FeatherYuba" };
      //var watershedNames = new string[] {  "EastSierra", "FeatherYuba" };
      var sw = new Stopwatch();
      foreach (var name in watershedNames)
      {
        var t1 = new DateTime(2013, 11, 3, 12, 0, 0);
        var t2 = new DateTime(2014, 11, 3, 12, 0, 0);
        var ws = r.Read(name, t1, t2);

        sw.Start();
       
        SqLiteEnsemble.Write(fn, ws, true, piscesFormat);
        sw.Stop();
      }

      Console.WriteLine(sw.Elapsed.TotalSeconds.ToString("F1")+" sqlite elapsed seconds");
      Console.WriteLine(swTotal.Elapsed.TotalSeconds.ToString("F1")+" Total elapsed seconds");

    }
  }
}
