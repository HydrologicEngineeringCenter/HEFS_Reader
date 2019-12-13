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
using Reclamation.TimeSeries;

namespace Hec.TimeSeries.Ensemble
{
  /// <summary>
  /// Reads/Writes Ensemble data to SQL tables
  /// each ensemble member is written to a blob
  /// with optional compressions
  /// </summary>
  public class SqLiteEnsemble
  {

    static SQLiteServer GetServer(string filename)
    {
      string connectionString = "Data Source=" + filename + ";Synchronous=Off;Pooling=True;Journal Mode=Off";
      SQLiteServer server = new SQLiteServer(connectionString);
      server.CloseAllConnections();
      return server;
    }



    static string TableName = "timeseries_blob";
    //static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    public static void Write(string filename, Watershed watershed, bool compress = false, bool createPiscesDB = false)
    {
      var server = SqLiteEnsemble.GetServer(filename);
      int index = 0;
      byte[] uncompressed = null;

      Reclamation.TimeSeries.TimeSeriesDatabase db;
      int locIdx = 1;
      int WatershedFolderIndex = 1;
      int scIndex = 0;
      int rowCounter = 0;
      Reclamation.TimeSeries.TimeSeriesDatabaseDataSet.SeriesCatalogDataTable sc = null;

      if (createPiscesDB)
      {
        db = new Reclamation.TimeSeries.TimeSeriesDatabase(server);
        // limit how much we query.
        //var where = "id = (select max(id) from seriescatalog) or id = parentid";
        var where = "id = (select max(id) from seriescatalog)";
        sc = db.GetSeriesCatalog(where);
        WatershedFolderIndex = sc.AddFolder(watershed.Name); // creates root level folder
        scIndex = WatershedFolderIndex + 2;
      }

      var timeSeriesTable = GetEmptyBlobTable(server);

      foreach (Location loc in watershed.Locations)
      {

        if (createPiscesDB)
        {
          locIdx = sc.AddFolder(loc.Name, ++scIndex,WatershedFolderIndex);
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
            string connectionString = "timeseries_blobs.id=" + index
        + ";member_length=" + f.Ensemble.GetLength(1)
        + ";ensemble_member_index={member_index}" 
        + ";timeseries_start_date=" + timeseries_start_date.ToString("yyyy-MM-dd HH:mm:ss");
            scIndex = AddPiscesSeries(loc.Name, scIndex, sc, f, locIdx, connectionString);
          }

          timeSeriesTable.Rows.Add(row);
          rowCounter++;
          if( rowCounter %1000 == 0)
          {
            server.SaveTable(timeSeriesTable);
            timeSeriesTable.Rows.Clear();
            timeSeriesTable.AcceptChanges();
          }
        }
        
      }
      if (createPiscesDB)
        server.SaveTable(sc);

      server.SaveTable(timeSeriesTable);
    }

    private static int AddPiscesSeries(string name, int scIndex, 
      TimeSeriesDatabaseDataSet.SeriesCatalogDataTable sc, Forecast f, int parentID, string connectionString)
    {
      string folderName = f.IssueDate.ToString("yyyy-MM-dd");
      int folderID =++scIndex;
      sc.AddFolder(folderName, folderID, parentID);

      int memberCount = f.Ensemble.GetLength(0);
      for (int i = 0; i < memberCount; i++)
      {
        var scrow = sc.NewSeriesCatalogRow();
        scrow.id = ++scIndex;
        scrow.Provider = "EnsembleSeries";
        scrow.ConnectionString = connectionString.Replace("{member_index}",(i+1).ToString());
        scrow.ParentID = folderID;
        scrow.siteid = name;
        scrow.Name = name + " member" + (i + 1);
        scrow.TimeInterval = Reclamation.TimeSeries.TimeInterval.Hourly.ToString();
        scrow.Units = "cfs";
        scrow.Parameter = "flow";

        sc.Rows.Add(scrow);
      }

      return scIndex;
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


     static byte[] Compress(byte[] bytes)
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
