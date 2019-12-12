using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hec.TimeSeries.Ensemble;
using System.IO;
using DSSIO;
using System.Linq;
using Reclamation.Core;

namespace EnsembleUnitTests
{
  [TestClass]
  public class PiscesTest
  {

    /// <summary>
    /// Create a simple ensemble with two members
    /// save to pisces then read back
    /// </summary>
    [TestMethod]
    public void Simple()
    {
      var fn = @"c:\temp\pisces-ensemble-simple.pdb";
      string fileName = Path.Combine(EnsembleTester.CacheDir, "test.csv");
      RfcCsvFile csv = new RfcCsvFile(fileName);

      float[,] data = csv.GetEnsemble("SCRN2");
       Watershed ws = new Watershed("simple");
      DateTime t = DateTime.Now.Date.AddHours(12);
      ws.AddForecast("scrn2", t, data, new DateTime[] { t });
      SqLiteEnsemble.Write(fn, ws, true, true);


    }

    [TestMethod]
    public void GenerateFromCsv()
    {

      var fn = @"c:\temp\pisces-ensemble-test.pdb";
      File.Delete(fn);
      CsvEnsembleReader r = new CsvEnsembleReader(EnsembleTester.CacheDir);

      var watershedNames = new string[] { "RussianNapa", "EastSierra", "FeatherYuba" };
      //var watershedNames = new string[] {  "EastSierra", "FeatherYuba" };

      foreach (var name in watershedNames)
      {
        var t1 = new DateTime(2013, 11, 3, 12, 0, 0);
        var ws = r.Read(name, t1, t1.AddDays(40));

        SqLiteEnsemble.Write(fn, ws, true, true);
      }

    }
  }
}
