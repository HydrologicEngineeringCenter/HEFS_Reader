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
  class SqlBlobEnsemble
  {

    static string tableName = "timeseries_hourly";
    public static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";


    internal static TimeSpan Write(SQLiteServer server, ITimeSeriesOfEnsembleLocations watersheds,
      bool compress = false)
    {
      Stopwatch sw = Stopwatch.StartNew();
      int index = 0;
      var timeSeriesTable = GetHourlyBlobTable(server);

      var newRowLock = new object();

      foreach (IWatershedForecast watershed in watersheds.Forecasts)
      {
        foreach (IEnsemble e in watershed.Locations)
        {
          var t = e.IssueDate;
          index++;
          var row = timeSeriesTable.NewRow();// create rows in separate loop first
          row["id"] = index;
          row["issue_date"] = e.IssueDate;
          row["watershed"] = watershed.WatershedName;
          row["location_name"] = e.LocationName;
          row["timeseries_start_date"] = e.Members[0].Times[0];
          row["timeseries_time_length"] = e.Members[0].Times.Length;
          row["binary_values"] = ConvertToBytes(e.Members, compress);

          lock (newRowLock)
          {
            timeSeriesTable.Rows.Add(row); // create rows in separate loop first
          }
        }
      }
      server.SaveTable(timeSeriesTable);
      sw.Stop();
      return sw.Elapsed;
    }

    private static byte[] ConvertToBytes(IList<IEnsembleMember> ensembleMembers, bool compress)
    {//https://stackoverflow.com/questions/6952923/conversion-double-array-to-byte-array
      float[] values = ensembleMembers[0].Values;
      var numBytesPerMember = values.Length * sizeof(float);
      var uncompressed = new byte[numBytesPerMember * ensembleMembers.Count];

      for (int i = 0; i < ensembleMembers.Count; i++)
      {
        values = ensembleMembers[i].Values;
        Buffer.BlockCopy(values, 0, uncompressed, i * numBytesPerMember, numBytesPerMember);
      }

      if (!compress)
        return uncompressed;
      var compressed = Compress(uncompressed);
      //double pct = (double)uncompressed.Length / (double)compressed.Length * 100;
      //Console.WriteLine("uncompressed: "+uncompressed.Length+"  compressed "+compressed.Length+ " "+pct);
      return compressed;
    }

    private static byte[] Compress(byte[] bytes)
    {
      using (var msi = new MemoryStream(bytes))
      using (var mso = new MemoryStream())
      {
        using (var gs = new GZipStream(mso, CompressionMode.Compress))
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
    /// Reads a set of ensembles based on criteria
    /// </summary>
    /// <param name="server"></param>
    /// <param name="issueStartTime"></param>
    /// <param name="issueEndTime"></param>
    /// <param name="watershedFilter"></param>
    /// <param name="locations">list of locations ('NVRC1','LAMC1',...) </param>
    /// <returns></returns>
    internal static IList<IList<HEFS_Reader.Interfaces.IEnsemble>> Read(Reclamation.Core.BasicDBServer server,
            DateTime issueStartTime, DateTime issueEndTime, string[] locations, string watershedFilter = "RussianNapa")
    {
      string sql = "Select * from " + tableName;
      sql += " WHERE issue_date >= " + server.PortableDateString(issueStartTime, DateTimeFormat)
       + " AND "
      + " issue_date <= " + server.PortableDateString(issueEndTime, DateTimeFormat);

      if (locations.Length > 0)
        sql += " AND location_name in '" + String.Join("','", locations) + "'";

      sql += " order by watershed,issue_date,location_name";

      var hourly_table = server.Table(tableName, sql);
      TimeSeriesOfEnsembleLocations tsoe = new TimeSeriesOfEnsembleLocations();

      List<IList<HEFS_Reader.Interfaces.IEnsemble>> output = new List<IList<HEFS_Reader.Interfaces.IEnsemble>>();

      foreach (DataRow row in hourly_table.Rows)
      {

        List<DateTime> times = GetTimes(row);
        List<List<float>> values = GetValues(row);

        Ensemble ensemeble = new Ensemble(row["location_name"].ToString(),
                                           (DateTime)row["issue_date"],
                                           values,
                                           times);
      }

      return output;
    }

    private static List<DateTime> GetTimes(DataRow row)
    {
      throw new NotImplementedException();
    }

    private static List<List<float>> GetValues(DataRow row)
    {
      var rval = new List<List<float>>();
      int size = Convert.ToInt32(row["timeseries_time_length"]);
      var numBytesPerMember = size * sizeof(float);

      byte[] binary_values = (byte[])row["binary_values"];
      int numMembers = binary_values.Length / numBytesPerMember;

      for (int i = 0; i < numMembers; i++)
      {
        var floatValues = new float[size];
        Buffer.BlockCopy(binary_values, i * numBytesPerMember, floatValues, 0, numBytesPerMember);
        var values = new List<float>();
        values.AddRange(floatValues);
        rval.Add(values);
      }

      return rval;
    }


    /// <summary>
    /// Returns and empty timeseries_hourly table for storing blobs
    /// </summary>
    /// <param name="server"></param>
    /// <returns></returns>
    static DataTable GetHourlyBlobTable(Reclamation.Core.BasicDBServer server)
    {


      if (!server.TableExists(tableName))
      {
        string sql = "CREATE TABLE " + tableName
        + " ( id integer not null primary key,"
        + "    issue_date datetime, "
        + "   watershed NVARCHAR(100) ,"
        + "   location_name NVARCHAR(100) ,"
        + "   timeseries_start_date datetime ,"
        + "   timeseries_time_length integer    ,"
        + "  binary_values BLOB NULL )";
        server.RunSqlCommand(sql);

      }
      server.RunSqlCommand("DELETE from " + tableName);
      return server.Table(tableName, "select * from " + tableName + " where 1=0");


    }




  }
}
