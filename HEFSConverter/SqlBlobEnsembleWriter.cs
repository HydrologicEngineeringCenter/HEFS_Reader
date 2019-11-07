using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HEFS_Reader.Implementations;
using HEFS_Reader.Interfaces;
using Reclamation.Core;

namespace HEFSConverter
{
  /// <summary>
  /// Writes HEFS data to SQL tables
  /// each ensemble member is written to a blob
  /// with optional compressions
  /// </summary>
  class SqlBlobEnsembleWriter
  {

    public static string tableName = "timeseries_blob";
    public static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";


    internal static TimeSpan Write(SQLiteServer server, ITimeSeriesOfEnsembleLocations watersheds,
      bool compress = false, bool createPiscesDB = false)
    {
      Stopwatch sw = Stopwatch.StartNew();
      int index = 0;

      Reclamation.TimeSeries.TimeSeriesDatabase db = null;
      int folderIndex = 1;
      int scIndex = 0;
      Reclamation.TimeSeries.TimeSeriesDatabaseDataSet.SeriesCatalogDataTable sc = null;

      if (createPiscesDB)
      {
        db = new Reclamation.TimeSeries.TimeSeriesDatabase(server);
        sc = db.GetSeriesCatalog();
        folderIndex = 1;
        scIndex = sc.NextID();
      }
      var timeSeriesTable = CreateAndGetBlobTable(server);

      //var newRowLock = new object();

      foreach (IWatershedForecast watershed in watersheds.Forecasts)
      {
        if (createPiscesDB)
        {
          folderIndex = sc.AddFolder(watershed.WatershedName.ToString(), scIndex++, folderIndex);
        }
        foreach (IEnsemble e in watershed.Locations)
        {
          var t = e.IssueDate;
          var timeseries_start_date = e.Members[0].Times[0];

          index++;
          var row = timeSeriesTable.NewRow();// create rows in separate loop first
          row["id"] = index;
          row["issue_date"] = e.IssueDate;
          row["watershed"] = watershed.WatershedName;
          row["location_name"] = e.LocationName;
          row["timeseries_start_date"] = timeseries_start_date;
          row["member_length"] = e.Members[0].Values.Length;
          row["member_count"] = e.Members.Count;
          row["compressed"] = compress ? 1 : 0;
          row["byte_value_array"] = ConvertToBytes(e.Members, compress);

          if (createPiscesDB)
          {
           int folderID= sc.GetOrCreateFolder(watershed.WatershedName.ToString(),e.LocationName, e.IssueDate.ToString("yyyy-MM-dd"));
            scIndex += 3;// increment enough for folders
            for (int i = 0; i < e.Members.Count; i++)
            {
              var ps = new Reclamation.TimeSeries.EnsembleSeries(timeseries_start_date, index, i);

              var scrow = sc.NewSeriesCatalogRow();
              scrow.id = scIndex++;
              scrow.Provider = "EnsembleSeries";
              scrow.ConnectionString = ps.ConnectionString;
              scrow.ParentID = folderID;
              scrow.siteid = e.LocationName;
              scrow.Name = "member" + (i + 1);
              scrow.TimeInterval = Reclamation.TimeSeries.TimeInterval.Hourly.ToString();
              scrow.Units = "cfs";
              scrow.Parameter = "flow";

              sc.Rows.Add(scrow);

            }

          }

          // lock (newRowLock)
          //{
          timeSeriesTable.Rows.Add(row); // create rows in separate loop first
                                         // }
        }
      }
      if (createPiscesDB)
        server.SaveTable(sc);
      server.SaveTable(timeSeriesTable);
      sw.Stop();
      return sw.Elapsed;
    }

    private static byte[] ConvertToBytes(IList<IEnsembleMember> ensembleMembers, bool compress)
    {//https://stackoverflow.com/questions/6952923/conversion-double-array-to-byte-array
      float[] values = ensembleMembers[0].Values.ToArray();
      var numBytesPerMember = values.Length * sizeof(float);
      var uncompressed = new byte[numBytesPerMember * ensembleMembers.Count];

      for (int i = 0; i < ensembleMembers.Count; i++)
      {
        values = ensembleMembers[i].Values.ToArray();
        Buffer.BlockCopy(values, 0, uncompressed, i * numBytesPerMember, numBytesPerMember);
      }

      if (!compress)
        return uncompressed;
      var compressed = Compress(uncompressed);
      //double pct = (double)uncompressed.Length / (double)compressed.Length * 100;
      //Console.WriteLine("uncompressed: "+uncompressed.Length+"  compressed "+compressed.Length+ " "+pct);
      return compressed;
    }


    public static byte[] Compress(byte[] bytes)
    {
      using (var msi = new MemoryStream(bytes))
      using (var mso = new MemoryStream())
      {
        var mode = CompressionMode.Compress;
        using (var gs = new GZipStream(mso, mode))
        {
          //msi.CopyTo(gs);
          CopyTo(msi, gs);
        }

        return mso.ToArray();
      }
    }
    private static void CopyTo(Stream src, Stream dest)
    {
      byte[] bytes = new byte[4096];

      int cnt;

      while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
      {
        dest.Write(bytes, 0, cnt);
      }
    }



    /// <summary>
    /// Returns and empty timeseries_hourly table for storing blobs
    /// </summary>
    /// <param name="server"></param>
    /// <returns></returns>
    static DataTable CreateAndGetBlobTable(Reclamation.Core.BasicDBServer server)
    {


      if (!server.TableExists(tableName))
      {
        string sql = "CREATE TABLE " + tableName
        + " ( id integer not null primary key,"
        + "    issue_date datetime, "
        + "   watershed NVARCHAR(100) ,"
        + "   location_name NVARCHAR(100) ,"
        + "   timeseries_start_date datetime ,"
        + "   member_length integer    ,"
        + "   member_count integer    ,"
        + "   compressed integer    ,"
        + "  byte_value_array BLOB NULL )";
        server.RunSqlCommand(sql);

      }
      server.RunSqlCommand("DELETE from " + tableName);
      return server.Table(tableName, "select * from " + tableName + " where 1=0");


    }




  }
}
