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
    public void Profiles()
    {
      string fileName = Path.Combine(EnsembleTester.CacheDir, "test.csv");
      RfcCsvFile csv = new RfcCsvFile(fileName);

      float[,] ensemble = null;

      csv.GetEnsemble("SCRN2", ref ensemble, true);

      var fn = @"C:\temp\dss7_profile_ensemble.dss";
      File.Delete(fn);
      using (DSSWriter w = new DSSWriter(fn))
      {
        TimeSeriesProfile ts = new TimeSeriesProfile();
        ts.StartDateTime = DateTime.Parse("2001-11-15");
        var IssueDate = ts.StartDateTime;
        //  /RUSSIANNAPA/APCC1/Ensemble-FLOW/01SEP2019/1HOUR/T:0212019/
        string F = "|T:" + IssueDate.DayOfYear.ToString().PadLeft(3, '0') + IssueDate.Year.ToString();
        var path = "/watershed/location/Ensemble-Flow//1Hour/" + F + "/";

        ts.ColumnValues = Array.ConvertAll(Enumerable.Range(1, ensemble.GetLength(1)).ToArray(), x => (double)x);
        ts.DataType = "INST-VAL";
        ts.Path = path;
        // convert to double
        double[,] d = new double[ensemble.GetLength(0), ensemble.GetLength(1)];
         Array.Copy(ensemble, d, ensemble.Length);
        ts.Values = d;
        bool saveAsFloat = true;
        w.Write(ts, saveAsFloat);

        var tp = w.GetTimeSeriesProfile(path);
        var data = tp.Values;
        int member = 0;
        Assert.AreEqual(-1.0f, data[0, 0]);
        Assert.AreEqual(-2.1f, data[1, 0]);
        Assert.AreEqual(-3.1f, data[2,0]);
        member = 58;
        Assert.AreEqual(-59.0f, data[0,member]);
        Assert.AreEqual(-59.1f, data[1,member]);
        Assert.AreEqual(-59.2f, data[2,member]);
      }




    }
  }
}
