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
    public static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
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
      using (SqLiteEnsembleWriter server = new SqLiteEnsembleWriter(filename))
      {
        int index = server.MaxID();

        byte[] uncompressed = null;

        Reclamation.TimeSeries.TimeSeriesDatabase db;

        foreach (Location loc in watershed.Locations)
        {
          foreach (Forecast f in loc.Forecasts)
          {
            index++;
            server.InsertEnsemble(index, f.IssueDate, watershed.Name, loc.Name, f.TimeStamps[0],
              f.Ensemble.GetLength(1), f.Ensemble.GetLength(0), compress, ConvertToBytes(f.Ensemble, compress, ref uncompressed));
          }

        }
      }
    }

    public static void WriteWithDataTable(string filename, Watershed watershed, bool compress = false, bool createPiscesDB = false)
    {
      var server = SqLiteEnsemble.GetServer(filename);
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
      else
      {

      }

      var timeSeriesTable = GetBlobTable(server);
      int index = server.NextID("timeseries_blob", "id");
      foreach (Location loc in watershed.Locations)
      {

        if (createPiscesDB)
        {
          locIdx = sc.AddFolder(loc.Name, ++scIndex, WatershedFolderIndex);
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
          if (rowCounter % 1000 == 0)
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
      int folderID = ++scIndex;
      sc.AddFolder(folderName, folderID, parentID);

      int memberCount = f.Ensemble.GetLength(0);
      for (int i = 0; i < memberCount; i++)
      {
        var scrow = sc.NewSeriesCatalogRow();
        scrow.id = ++scIndex;
        scrow.Provider = "EnsembleSeries";
        scrow.ConnectionString = connectionString.Replace("{member_index}", (i + 1).ToString());
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

      if (uncompressed == null || uncompressed.Length != ensemble.Length)
        uncompressed = new byte[width * height * sizeof(float)];

      Buffer.BlockCopy(ensemble, 0, uncompressed, 0, uncompressed.Length);

      if (!compress)
        return uncompressed;
      var compressed = Compress(uncompressed);
      return compressed;
    }

    public static Watershed Read(string watershedName, DateTime startTime, DateTime endTime, string fileName)
    {
      SQLiteServer server = GetServer(fileName);
      var rval = new Watershed(watershedName);

      var sql = "select * from " + TableName +
        " WHERE issue_date >= '" + startTime.ToString(DateTimeFormat) + "' "
        + " AND issue_date <= '" + endTime.ToString(DateTimeFormat) + "' "
        + " AND watershed = '" + watershedName + "' ";
      sql += " order by watershed,issue_date,location_name";

      var table = server.Table(TableName, sql);
      if (table.Rows.Count == 0)
      {
        throw new Exception("no data");
      }
      DateTime prevIssueDate = Convert.ToDateTime(table.Rows[0]["issue_date"]);
      DateTime currentDate = Convert.ToDateTime(table.Rows[0]["issue_date"]);
      float[,] values = null;
      foreach (DataRow row in table.Rows)
      {
        currentDate = Convert.ToDateTime(row["issue_date"]);

        var times = GetTimes(row);
        GetValues(row, ref values);

        rval.AddForecast(row["location_name"].ToString(),
                                             currentDate,
                                             values,
                                             times);

      }
      return rval;
    }
    private static DateTime[] GetTimes(DataRow row)
    {
      DateTime t = Convert.ToDateTime(row["timeseries_start_date"]);
      int count = Convert.ToInt32(row["member_length"]);
      var rval = new DateTime[count];
      for (int i = 0; i < count; i++)
      {
        rval[i] = t;
        t = t.AddHours(1); // hardcode hourly
      }
      return rval;
    }

    //https://stackoverflow.com/questions/7013771/decompress-byte-array-to-string-via-binaryreader-yields-empty-string
    static byte[] Decompress(byte[] data)
    {
      // Was previously a GZip stream
      using (var compressedStream = new MemoryStream(data))
      using (var zipStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
      using (var resultStream = new MemoryStream())
      {
        zipStream.CopyTo(resultStream);
        return resultStream.ToArray();
      }
    }

    private static void GetValues(DataRow row, ref float[,] data)
    {
      int compressed = Convert.ToInt32(row["compressed"]);
      var rval = new List<List<float>>();
      int member_count = Convert.ToInt32(row["member_count"]);
      int member_length = Convert.ToInt32(row["member_length"]);

      byte[] byte_values = (byte[])row["byte_value_array"];

      if (compressed != 0)
      {
        byte_values = Decompress(byte_values);
      }

      if (data == null || data.GetLength(0) != member_count || data.GetLength(1) != member_length)
        data = new float[member_count, member_length];

      var numBytesPerMember = byte_values.Length / member_count;

      Buffer.BlockCopy(byte_values, 0, data, 0, data.Length * sizeof(float));

      //for (int i = 0; i < member_count; i++)
      //{
      //  var floatValues = new float[member_length];
      //  Buffer.BlockCopy(byte_values, i * numBytesPerMember, floatValues, 0, numBytesPerMember);
      //  var values = new List<float>();
      //  values.AddRange(floatValues);
      //  rval.Add(values);
      //}

      // return rval;
    }



    static byte[] Compress(byte[] bytes)
    {
      using (var msi = new MemoryStream(bytes))
      using (var mso = new MemoryStream())
      {
        //var mode = CompressionMode.Compress;
        // using (var gs = new GZipStream(mso, mode))

        // Fastest deflatestream compression to match Alex's internal HDF5 deflate
        using (var gs = new DeflateStream(mso, CompressionLevel.Fastest))
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
    static DataTable GetBlobTable(Reclamation.Core.BasicDBServer server)
    {
      string sql = SqLiteEnsembleWriter.GetCreateTableSQL(TableName);
      server.RunSqlCommand(sql);

      //server.RunSqlCommand("DELETE from " + TableName);
      return server.Table(TableName, "select * from " + TableName + " where 1=0");
    }


  }
}
