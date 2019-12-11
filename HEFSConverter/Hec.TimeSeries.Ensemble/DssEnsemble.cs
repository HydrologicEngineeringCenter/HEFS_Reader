using DSSIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hec.TimeSeries.Ensemble;

namespace Hec.TimeSeries.Ensemble
{
  public class DssEnsemble
  {
    public static void Write(string dssFileName, Watershed watershed)
    {
      bool saveAsFloat = true;

      Console.WriteLine("Saving to " + dssFileName);
      int count = 0;
      using (var w = new DSSWriter(dssFileName, DSSReader.MethodID.MESS_METHOD_GLOBAL_ID, DSSReader.LevelID.MESS_LEVEL_NONE))
      {
        foreach (Location loc in watershed.Locations)
        {
          if (count % 100 == 0)
            Console.Write(".");
          int memberCounter = 0;

          foreach (Forecast f in loc.Forecasts)
          {
            int size = f.Ensemble.GetLength(0);
            for (int i = 0; i < size; i++)
            {
              float[] ensembleMember = f.EnsembleMember(i);

              memberCounter++;
              ///   A/B/FLOW//1 Hour/<FPART></FPART>
              //// c:  ensemble.p
              var t = f.IssueDate;
              //  /RUSSIANNAPA/APCC1/FLOW/01SEP2019/1HOUR/C:000002|T:0212019/
              string F = "C:" + memberCounter.ToString().PadLeft(6, '0') + "|T:" +
                    t.DayOfYear.ToString().PadLeft(3, '0') + t.Year.ToString();

              var path = "/" + watershed.Name.ToString() + "/" + loc.Name + "/Flow//1Hour/" + F + "/";
              DSSTimeSeries timeseries = new DSSTimeSeries
              {
                Values = Array.ConvertAll(ensembleMember, item => (double)item),
                Units = "",
                DataType = "INST-VAL",
                Path = path,
                StartDateTime = f.TimeStamps[0]
              };
              w.Write(timeseries, saveAsFloat);
              count++;
            }
          }
        }
      }
    }

    /// <summary>
    /// /RUSSIANNAPA/APCC1/ensemble-FLOW/01SEP2019/1HOUR/T:0212019/
    /// </summary>
    /// <param name="dssFileName"></param>
    /// <param name="watersheds"></param>
    /// <param name="saveAsFloat"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    internal static void WriteToTimeSeriesProfiles(string dssFileName, Watershed watershed)
    {
      bool saveAsFloat = true;

      Console.WriteLine("Saving to " + dssFileName);
      int count = 0;
      using (var w = new DSSWriter(dssFileName, DSSReader.MethodID.MESS_METHOD_GLOBAL_ID, DSSReader.LevelID.MESS_LEVEL_NONE))
      {
        foreach (Location loc in watershed.Locations)
        {
          foreach (Forecast f in loc.Forecasts)
          {
            float[,] ensemble = f.Ensemble;
            if (count % 100 == 0)
              Console.Write(".");

            TimeSeriesProfile ts = new TimeSeriesProfile();
            ts.StartDateTime = f.IssueDate;

            //  /RUSSIANNAPA/APCC1/Ensemble-FLOW/01SEP2019/1HOUR/T:0212019/
            string F = "|T:" + f.IssueDate.DayOfYear.ToString().PadLeft(3, '0') + f.IssueDate.Year.ToString();
            var path = "/" + watershed.Name.ToString() + "/" + loc.Name + "/Ensemble-Flow//1Hour/" + F + "/";

            ts.ColumnValues = Array.ConvertAll(Enumerable.Range(1, ensemble.GetLength(0)).ToArray(), x => (double)x);
            ts.DataType = "INST-VAL";
            ts.Path = path;
            int numColumns = ensemble.GetLength(0);
            int numRows = ensemble.GetLength(1);
            double[,] d = new double[ensemble.GetLength(0), ensemble.GetLength(1)];
            Array.Copy(ensemble, d, ensemble.Length);
            ts.Values = d;

            w.Write(ts, saveAsFloat);
            count++;

          }
        }
      }
    }

  }
}

