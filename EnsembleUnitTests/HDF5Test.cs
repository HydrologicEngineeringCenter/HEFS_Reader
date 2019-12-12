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
    public void Simple()
    {
      var fn = @"c:\temp\ensemble-simple.h5";
      string fileName = Path.Combine(EnsembleTester.CacheDir, "test.csv");
      RfcCsvFile csv = new RfcCsvFile(fileName);

      float[,] data = csv.GetEnsemble("SCRN2");
       Watershed ws = new Watershed("simple");
      DateTime t = DateTime.Now.Date.AddHours(12);
      ws.AddForecast("scrn2", t, data, new DateTime[] { t });

      H5Assist.H5Writer h5 = new H5Assist.H5Writer(fn);
      HDF5ReaderWriter.Write(h5,ws);

    }

  }
}
