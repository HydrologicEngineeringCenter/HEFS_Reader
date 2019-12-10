using DSSIO;
using HEFS_Reader.Implementations;
using HEFS_Reader.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFSConverter
{
  public class DssEnsembleWriter
  {
    public static TimeSpan Write(string dssFileName, TimeSeriesOfEnsembleLocations watersheds, bool saveAsFloat, int version)
    {
      var sw = Stopwatch.StartNew();

      File.Delete(dssFileName);
      Hec.Dss.DSS.ZSet("DSSV", "", version);

      Console.WriteLine("Saving to "+dssFileName);
      int count = 0;
      using (var w = new DSSWriter(dssFileName,DSSReader.MethodID.MESS_METHOD_GLOBAL_ID,DSSReader.LevelID.MESS_LEVEL_NONE))
      {
        foreach (WatershedForecast watershed in watersheds.Forecasts)
        {
          foreach (Ensemble e in watershed.Locations)
          {
           if( count %100 == 0)
              Console.Write(".");
            int memberCounter = 0;
            foreach (EnsembleMember m in e.Members)
            {
              memberCounter++;
              ///   A/B/FLOW//1 Hour/<FPART></FPART>
              //// c:  ensemble.p
              var t = e.IssueDate;
              //  /RUSSIANNAPA/APCC1/FLOW/01SEP2019/1HOUR/C:000002|T:0212019/
              string F = "C:" + memberCounter.ToString().PadLeft(6, '0') + "|T:" +
                    t.DayOfYear.ToString().PadLeft(3, '0') + t.Year.ToString();

              var path = "/" + watershed.WatershedName.ToString() + "/" + e.LocationName + "/Flow//1Hour/" + F + "/";
              DSSTimeSeries timeseries = new DSSTimeSeries
              {
                Values = Array.ConvertAll(m.Values.ToArray(), item => (double)item),
                Units = "",
                DataType = "INST-VAL",
                Path = path,
                StartDateTime = m.Times[0]
              };
              //Console.WriteLine("saving: " + path);
              w.Write(timeseries, saveAsFloat);
              count++;
              //                            int status = w.StoreTimeSeriesRegular(path, d, 0, DateTime.Now.Date, "cfs", "INST-VAL");

            }
          }
        }

      }
      sw.Stop();
      Console.WriteLine();
      return sw.Elapsed;
    }

    /// <summary>
    /// /RUSSIANNAPA/APCC1/ensemble-FLOW/01SEP2019/1HOUR/T:0212019/
    /// </summary>
    /// <param name="dssFileName"></param>
    /// <param name="watersheds"></param>
    /// <param name="saveAsFloat"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    internal static TimeSpan WriteToTimeSeriesProfiles(string dssFileName, TimeSeriesOfEnsembleLocations watersheds, bool saveAsFloat, int version)
    {
      var sw = Stopwatch.StartNew();


      File.Delete(dssFileName);
      Hec.Dss.DSS.ZSet("DSSV", "", version);

      Console.WriteLine("Saving to " + dssFileName);
      int count = 0;
      using (var w = new DSSWriter(dssFileName, DSSReader.MethodID.MESS_METHOD_GLOBAL_ID, DSSReader.LevelID.MESS_LEVEL_NONE))
      {
        foreach (WatershedForecast watershed in watersheds.Forecasts)
        {
          foreach (Ensemble e in watershed.Locations)
          {
            if (count % 100 == 0)
              Console.Write(".");
            
            TimeSeriesProfile ts = new TimeSeriesProfile();
            ts.StartDateTime = e.IssueDate;
            
            //  /RUSSIANNAPA/APCC1/Ensemble-FLOW/01SEP2019/1HOUR/T:0212019/
            string F = "|T:" + e.IssueDate.DayOfYear.ToString().PadLeft(3, '0') + e.IssueDate.Year.ToString();
            var path = "/" + watershed.WatershedName.ToString() + "/" + e.LocationName + "/Ensemble-Flow//1Hour/" + F + "/";

            ts.ColumnValues = Array.ConvertAll(Enumerable.Range(1, e.Members.Count).ToArray(), x => (double)x);
            ts.DataType = "INST-VAL";
            ts.Path = path;
            int numColumns = ts.ColumnValues.Length;
            int numRows = e.Members[0].Values.Length;
            ts.Values = ConvertEnsembleToArray(e, numColumns, numRows);

            w.Write(ts, saveAsFloat);
            count++;

          }
        }

      }
      sw.Stop();
      Console.WriteLine();
      return sw.Elapsed;
    }

    private static double[,] ConvertEnsembleToArray(Ensemble e, int numColumns, int numRows)
    {
      double[,] data = new double[numRows,numColumns];
      for (int i = 0; i < numColumns; i++)
      {
        var m = e.Members[i].Values;
        for (int r = 0; r < m.GetLength(0); r++)
        {
          data[r,i] = m[r];
        }
      }
      return data;
    }
  }
}
