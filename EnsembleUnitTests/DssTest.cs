using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hec.TimeSeries.Ensemble;
using System.IO;
using DSSIO;
using System.Linq;

namespace EnsembleUnitTests
{
  [TestClass]
  public class DssTest
  {
    [TestMethod]
    public void ProfileTesting()
    {
      var fn = @"C:\temp\dss7_profile_ensemble.dss";
      File.Delete(fn);


      string csvFilename = Path.Combine(EnsembleTester.CacheDir, "test.csv");
      RfcCsvFile csv = new RfcCsvFile(csvFilename);

      float[,] ensemble = null;

      csv.GetEnsemble("SCRN2", ref ensemble);

      Watershed ws = new Watershed("test");
      DateTime issueDate = new DateTime(2001,11,15,12,0,0);
      ws.AddForecast("SCRN2", issueDate, ensemble, csv.TimeStamps);

     
      DssEnsemble.WriteToTimeSeriesProfiles(fn, ws);

      var ws2 = DssEnsemble.ReadTimeSeriesProfiles("test", issueDate, issueDate, fn);
      Assert.AreEqual(1, ws2.Locations.Count);

      var data = ws2.Locations[0].Forecasts[0].Ensemble;
      Assert.AreEqual(-1.0f, data[0, 0]);
      Assert.AreEqual(-2.1f, data[0, 1]);
      Assert.AreEqual(-3.1f, data[0, 2]);
      Assert.AreEqual(-59.0f, data[58, 0]);
      Assert.AreEqual(-59.1f, data[58, 1]);
      Assert.AreEqual(-59.2f, data[58, 2]);

    }

    [TestMethod]
    public void TimeSeriesTesting()
    {
      var fn = @"C:\temp\dss7_ensemble.dss";
      File.Delete(fn);


      string fileName = Path.Combine(EnsembleTester.CacheDir, "test.csv");
      RfcCsvFile csv = new RfcCsvFile(fileName);
      float[,] ensemble = null;
      csv.GetEnsemble("SCRN2", ref ensemble);
      Watershed w = new Watershed("test");
      w.AddForecast("SCRN2", DateTime.Parse("2001-11-15"), ensemble, csv.TimeStamps);

      DssEnsemble.Write(fn, w);

      using (DSSReader r = new DSSReader(fn))
      {
        var catalog = r.GetCatalog();
        Assert.AreEqual(59, catalog.Count);
        string path = "/test/SCRN2/Flow/01Nov2013/1Hour/C:000059|T:3192001/";
        var ts = r.GetTimeSeries(path);
        Assert.AreEqual(-59.0, ts[0].Value);

      }

      DateTime t1 = new DateTime(1900, 1, 1);
      DateTime t2 = DateTime.Now.Date;
      w = DssEnsemble.Read("test", t1, t2, fn);

      Assert.AreEqual(1, w.Locations.Count);

      ensemble = w.Locations[0].Forecasts[0].Ensemble;
      Assert.AreEqual(337, ensemble.GetLength(1));
      Assert.AreEqual(59, ensemble.GetLength(0));


      Assert.AreEqual(-59.0f, ensemble[58, 0]);
      Assert.AreEqual(-59.1f, ensemble[58, 1]);

    }


  }
}

  