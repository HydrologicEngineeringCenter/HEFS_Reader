using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hec.TimeSeries.Ensemble;
using System.IO;

namespace EnsembleUnitTests
{
  [TestClass]
  public class CsvEnsembleTests
  {

    [TestMethod]
    public void EastSierra_SCRN2()
    {
      CsvEnsembleReader r = new CsvEnsembleReader(EnsembleTester.CacheDir);
    
      var t1 = new DateTime(2013, 11, 3, 12, 0, 0);
      var ws = r.Read("EastSierra", t1, t1.AddDays(1));

      var scrn2 = ws.Locations.Find(x => x.Name == "SCRN2");

      Console.WriteLine(scrn2);


    }

    [TestMethod]
    public void CsvTest()
    {
      string fileName = Path.Combine(EnsembleTester.CacheDir, "test.csv");
      MultiLocationRfcCsvFile csv = new MultiLocationRfcCsvFile(fileName);
      float[,] data = null;
       csv.GetEnsemble("SCRN2",ref data);
      Assert.AreEqual(-1.0f, data[0, 0]);
      Assert.AreEqual(-2.1f, data[0, 1]);
      Assert.AreEqual(-3.1f, data[0, 2]);
      Assert.AreEqual(-59.0f, data[58, 0]);
      Assert.AreEqual(-59.1f, data[58, 1]);
      Assert.AreEqual(-59.2f, data[58, 2]);


    }
  }
}
