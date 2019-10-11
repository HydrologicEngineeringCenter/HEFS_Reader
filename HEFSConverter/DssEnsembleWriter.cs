using DSSIO;
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
  class DssEnsembleWriter
  {
    internal static TimeSpan Write(string dssFileName, ITimeSeriesOfEnsembleLocations watersheds, bool saveAsFloat, int version)
    {
      var sw = Stopwatch.StartNew();


      File.Delete(dssFileName);
      Hec.Dss.DSS.ZSet("DSSV", "", version);

      int count = 0;
      using (var w = new DSSWriter(dssFileName))
      {
        foreach (IWatershedForecast watershed in watersheds.timeSeriesOfEnsembleLocations)
        {
          foreach (IEnsemble e in watershed.Locations)
          {
            int memberCounter = 0;
            foreach (IEnsembleMember m in e.Members)
            {
              memberCounter++;
              ///   A/B/FLOW//1 Hour/<FPART></FPART>
              //// c:  ensemble.p
              var t = e.IssueDate;
              //  /RUSSIANNAPA/APCC1/FLOW/01SEP2019/1HOUR/C:000002|T:0212019/
              string F = "C:" + memberCounter.ToString().PadLeft(6, '0') + "|T:" +
                    t.DayOfYear.ToString().PadLeft(3, '0') + t.Year.ToString();

              var path = "/" + watershed.WatershedName + "/" + e.LocationName + "/Flow//1Hour/" + F + "/";
              DSSTimeSeries timeseries = new DSSTimeSeries
              {
                Values = Array.ConvertAll(m.Values.ToArray(), item => (double)item),
                Units = "",
                DataType = "INST-VAL",
                Path = path,
                StartDateTime = m.Times[0]
              };
              Console.WriteLine("saving: " + path);
              w.Write(timeseries, saveAsFloat);
              count++;
              //                            int status = w.StoreTimeSeriesRegular(path, d, 0, DateTime.Now.Date, "cfs", "INST-VAL");

            }
          }
        }

      }
      sw.Stop();
      return sw.Elapsed;
    }

    
  }
}
