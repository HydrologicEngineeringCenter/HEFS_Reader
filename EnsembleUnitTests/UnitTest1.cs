using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hec.TimeSeries.Ensemble;
using System.IO;

namespace EnsembleUnitTests
{
  [TestClass]
  public class UnitTest1
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
      RfcCsvFile csv = new RfcCsvFile(fileName);

      var data = csv.GetEnsemble("SCRN2");
      Console.WriteLine(data);
    }
  }
}
