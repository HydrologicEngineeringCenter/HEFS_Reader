using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reclamation.Core;

namespace Hec.TimeSeries.Ensemble
{
  /// <summary>
  /// Writes HEFS data to SQL tables
  /// each ensemble member is written to a blob
  /// with optional compressions
  /// </summary>
  public class SqLiteEnsemble
  {
    public static string TableName = "timeseries_blob";
    public static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    internal static void Write(SQLiteServer server, Watershed watershed, bool compress = false, bool createPiscesDB = false)
    {
      int index = 0;
      byte[] uncompressed = null;

      Reclamation.TimeSeries.TimeSeriesDatabase db;
      int folderIndex = 1;
      int scIndex = 0;
      Reclamation.TimeSeries.TimeSeriesDatabaseDataSet.SeriesCatalogDataTable sc = null;

      if (createPiscesDB)
      {
        db = new Reclamation.TimeSeries.TimeSeriesDatabase(server);
        sc = db.GetSeriesCatalog();
        folderIndex = 1;
        scIndex = sc.NextID();
        folderIndex = sc.AddFolder(watershed.Name.ToString(), scIndex, scIndex);
        scIndex++;
      }

      var timeSeriesTable = GetEmptyBlobTable(server);

      //var newRowLock = new object();

      foreach (Location loc in watershed.Locations)
      {
        
          if (createPiscesDB)
          {
            folderIndex = sc.AddFolder(loc.Name.ToString(), scIndex++, folderIndex);
          }
        foreach (Forecast f in loc.Forecasts)
        {
          var t = f.IssueDate;
          var timeseries_start_date = f.TimeStamps[0];

          index++;
          var row = timeSeriesTable.NewRow();// create rows in separate loop first
          row["id"] = index;
          row["issue_date"] = f.IssueDate;
          row["watershed"] = watershed.Name;
          row["location_name"] = loc.Name;
          row["timeseries_start_date"] = timeseries_start_date;
          row["member_length"] = f.Ensemble.GetLength(1);
          row["member_count"] = f.Ensemble.GetLength(0);
          row["compressed"] = compress ? 1 : 0;
          row["byte_value_array"] = ConvertToBytes(f.Ensemble, compress, ref uncompressed);

          if (createPiscesDB)
          {
            int folderID = sc.GetOrCreateFolder(watershed.Name, loc.Name, f.IssueDate.ToString("yyyy-MM-dd"));
            scIndex += 3;// increment enough for folders
         
            for (int i = 0; i < f.Ensemble.GetLength(0); i++)
            {
              var ps = new Reclamation.TimeSeries.EnsembleSeries(timeseries_start_date, index, i);

              var scrow = sc.NewSeriesCatalogRow();
              scrow.id = scIndex++;
              scrow.Provider = "EnsembleSeries";
              scrow.ConnectionString = ps.ConnectionString;
              scrow.ParentID = folderID;
              scrow.siteid = loc.Name;
              scrow.Name = loc.Name+" member" + (i + 1);
              scrow.TimeInterval = Reclamation.TimeSeries.TimeInterval.Hourly.ToString();
              scrow.Units = "cfs";
              scrow.Parameter = "flow";

              sc.Rows.Add(scrow);
            }
          }

          timeSeriesTable.Rows.Add(row); 
        }
        
      }
      if (createPiscesDB)
        server.SaveTable(sc);

      server.SaveTable(timeSeriesTable);
    }

    
    static byte[] ConvertToBytes(float[,] ensemble, bool compress, ref byte[] uncompressed)
    {
      int width = ensemble.GetLength(1);
      int height = ensemble.GetLength(0);

      if(uncompressed == null || uncompressed.Length != ensemble.Length)
         uncompressed = new byte[width * height * sizeof(float)];

      Buffer.BlockCopy(ensemble, 0, uncompressed, 0, uncompressed.Length);

      if (!compress)
        return uncompressed;
      var compressed = Compress(uncompressed);
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
    static DataTable GetEmptyBlobTable(Reclamation.Core.BasicDBServer server)
    {
      if (!server.TableExists(TableName))
      {
        string sql = "CREATE TABLE " + TableName
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

      server.RunSqlCommand("DELETE from " + TableName);
      return server.Table(TableName, "select * from " + TableName + " where 1=0");
    }




  }
}
