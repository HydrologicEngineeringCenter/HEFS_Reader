using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HEFS_Reader.Interfaces;
using Reclamation.Core;

namespace HEFSConverter
{
    /// <summary>
    /// Writes HEFS data to SQL tables,
    /// table is named based on watershed_location_forecast
    /// columns for each ensemble member
    /// first column is date time
    /// </summary>
    class SqlEnsembleWriter
    {

    internal static TimeSpan Write(BasicDBServer server, ITimeSeriesOfEnsembleLocations waterShedData)
    {
      var sw = Stopwatch.StartNew();
      var enumerator = GetTableEnumerator(waterShedData);
      foreach (DataTable item in enumerator)
      {
        server.CreateTable(item);
        server.SaveTable(item);
      }

      sw.Stop();
      return sw.Elapsed ;
    }
    private static IEnumerable<DataTable> GetTableEnumerator(ITimeSeriesOfEnsembleLocations watersheds)
    {
      foreach (IWatershedForecast watershed in watersheds.timeSeriesOfEnsembleLocations)
      {
        foreach (IEnsemble e in watershed.Locations)
        {
          
          var t = e.IssueDate;
          DataTable tbl = new DataTable(watershed.WatershedName + "_" + e.LocationName + "_day_" + t.DayOfYear.ToString() + "_" + t.Year);
          tbl.Columns.Add("DateTime", typeof(DateTime));
           
          for (int ensembleMember = 1; ensembleMember <= e.Members.Count; ensembleMember++)
          {
            tbl.Columns.Add("member_" + ensembleMember.ToString(), typeof(double));
          }

          for (int ensembleMember = 0; ensembleMember < e.Members.Count; ensembleMember++)
          {
            var m = e.Members[ensembleMember];
            var vals = m.Values;
            for (int i = 0; i < vals.Count; i++)
            {
              if (ensembleMember == 0)
              {
                var row = tbl.NewRow();
                tbl.Rows.Add(row);
                DateTime[] times = m.Times.ToArray();
                row[0] = times[i];
              }
              tbl.Rows[i][ensembleMember + 1] = vals[i];
            }
          }
          yield return tbl;

        }
      }

    }



   
  }
}
