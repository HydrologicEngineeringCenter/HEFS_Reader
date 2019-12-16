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
    public void ReadCsvWriteSqlite()
    {
      var t = DateTime.Now.Date.AddHours(12);
      Watershed ws = ReadTestWaterShed( t);


      var fn = @"c:\temp\sql-ensemble-simple.pdb";
      RoundTrip(fn, ws,t,true);

      fn = @"c:\temp\pisces-ensemble-simple.pdb";
      RoundTrip(fn, ws, t, false);

    }

    private static void RoundTrip(string fn, Watershed ws, DateTime t, bool piscesFormat=false)
    {


      SqLiteEnsemble.Write(fn, ws, true, piscesFormat);

      var ws2 = SqLiteEnsemble.Read("simple", t, t.AddDays(1), fn);
      Assert.AreEqual(1, ws2.Locations.Count);

      var data = ws2.Locations[0].Forecasts[0].Ensemble;
      Assert.AreEqual(-1.0f, data[0, 0]);
      Assert.AreEqual(-2.1f, data[0, 1]);
      Assert.AreEqual(-3.1f, data[0, 2]);
      Assert.AreEqual(-59.0f, data[58, 0]);
      Assert.AreEqual(-59.1f, data[58, 1]);
      Assert.AreEqual(-59.2f, data[58, 2]);
    }

    private static Watershed ReadTestWaterShed( DateTime t)
    {
      string fileName = Path.Combine(EnsembleTester.CacheDir, "test.csv");
      RfcCsvFile csv = new RfcCsvFile(fileName);

      float[,] scrn2 = csv.GetEnsemble("SCRN2");
      var rval = new Watershed("simple");
      
      rval.AddForecast("scrn2", t, scrn2, new DateTime[] { t });
      return rval;
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
