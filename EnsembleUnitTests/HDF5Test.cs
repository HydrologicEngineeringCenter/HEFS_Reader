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
  public class HDF5Test
  {

    /// <summary>
    /// Create a simple ensemble with two members
    /// save to hdf5 then read back
    /// </summary>
    [TestMethod]
    public void SingleEnsemble()
    {
      var fn = @"c:\temp\ensemble-single.h5";
      File.Delete(fn);
      string fileName = Path.Combine(EnsembleTester.CacheDir, "test.csv");
      MultiLocationRfcCsvFile csv = new MultiLocationRfcCsvFile(fileName);

      float[,] data = csv.GetEnsemble("SCRN2");
       Watershed ws = new Watershed("simple");
      DateTime t = DateTime.Now.Date.AddHours(12);
      ws.AddForecast("scrn2", t, data, new DateTime[] { t });

      H5Assist.H5Writer h5 = new H5Assist.H5Writer(fn);
      HDF5Ensemble.Write(h5,ws);

      var w = HDF5Ensemble.Read(h5, "simple");
      Assert.AreEqual(1, w.Locations.Count);

      var ensemble = w.Locations[0].Forecasts[0].Ensemble;
      Assert.AreEqual(337, ensemble.GetLength(1));
      Assert.AreEqual(59, ensemble.GetLength(0));


      Assert.AreEqual(-59.0f, ensemble[58, 0]);
      Assert.AreEqual(-59.1f, ensemble[58, 1]);

    }

    [TestMethod]
    public void WholeForecast()
    {
      
      CsvEnsembleReader r = new CsvEnsembleReader(EnsembleTester.CacheDir);

      var t1 = new DateTime(2013, 11, 3, 12, 0, 0);
      var ws = r.Read("EastSierra", t1, t1.AddDays(1));

      var fn = @"c:\temp\ensemble-forecast.h5";
      File.Delete(fn);

      using (H5Assist.H5Writer h5 = new H5Assist.H5Writer(fn))
      {
        HDF5Ensemble.Write(h5, ws);
      }
      fn = @"c:\temp\ensemble-forecast-parallel.h5";
      File.Delete(fn);
      using (H5Assist.H5Writer h5 = new H5Assist.H5Writer(fn))
      {
        HDF5Ensemble.WriteParallel(h5, ws, -1);
      }
     
    


    }

  }
}
