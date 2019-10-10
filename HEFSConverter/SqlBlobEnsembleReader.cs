using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HEFS_Reader.Enumerations;
using HEFS_Reader.Implementations;
using HEFS_Reader.Interfaces;
using Reclamation.Core;

namespace HEFSConverter
{

 
  class SqlBlobEnsembleReader : HEFS_Reader.Interfaces.IEnsembleReader, HEFS_Reader.Interfaces.ITimeable
  {

    static string tableName = "timeseries_hourly";
    public static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    private long _readTimeInMilliSeconds = 0;
    public long ReadTimeInMilliSeconds { get { return _readTimeInMilliSeconds; } }

    public ITimeSeriesOfEnsembleLocations ReadDataset(Watersheds watershed, DateTime start, DateTime end, string Path)
    {
      var st = System.Diagnostics.Stopwatch.StartNew();
      var  connectionString = "Data Source=" + Path + ";Synchronous=Off;Pooling=True;Journal Mode=Off";

      SQLiteServer server = new SQLiteServer(connectionString);
      List<HEFS_Reader.Interfaces.IEnsemble> ensembles = new List<HEFS_Reader.Interfaces.IEnsemble>();
      TimeSeriesOfEnsembleLocations rval = new TimeSeriesOfEnsembleLocations();
      IList<IWatershedForecast> watershedForecasts = rval.timeSeriesOfEnsembleLocations;
     
      var sql = "select * from " + tableName +
        " WHERE issue_date >= '" + start.ToString(DateTimeFormat) + "' "
        + " AND issue_date <= '" + end.ToString(DateTimeFormat) + "' "
        + " AND watershed = '" + watershed.ToString() + "' ";
        sql += " order by watershed,issue_date,location_name";

      var table = server.Table(tableName, sql);
      if( table.Rows.Count == 0)
      {
        throw new Exception("no data");
      }
      DateTime prevIssueDate = Convert.ToDateTime(table.Rows[0]["issue_date"]);
      DateTime currentDate = Convert.ToDateTime(table.Rows[0]["issue_date"]);
      WatershedForecast watershedForecast = new WatershedForecast(ensembles, watershed); // one csv
      foreach (DataRow row in table.Rows)
      {
        currentDate = Convert.ToDateTime(row["issue_date"]);
         
        if( currentDate != prevIssueDate )// new issue_date
        {
          watershedForecasts.Add(watershedForecast); // one csv.. (forecast group)

          ensembles = new List<IEnsemble>();
          watershedForecast = new WatershedForecast(ensembles, watershed);
          prevIssueDate = currentDate;
        }
        List<DateTime> times = GetTimes(row);
        List<List<float>> values = GetValues(row);

        Ensemble ensemeble = new Ensemble(row["location_name"].ToString(),
                                             currentDate,
                                             values,
                                             times);

       
        ensembles.Add(ensemeble);  // one location.
      }
      watershedForecasts.Add(watershedForecast);
      st.Stop();
      _readTimeInMilliSeconds = st.ElapsedMilliseconds;

      return rval;
    }
    private static List<DateTime> GetTimes(DataRow row)
    {
      var rval = new List<DateTime>();
      DateTime t = Convert.ToDateTime(row["timeseries_start_date"]);
      int count = Convert.ToInt32(row["member_length"]);
      for (int i = 0; i < count; i++)
      {
        rval.Add(t);
        t = t.AddHours(1); // hardcode hourly
      }
      return rval;
    }

    //https://stackoverflow.com/questions/7013771/decompress-byte-array-to-string-via-binaryreader-yields-empty-string
    static byte[] Decompress(byte[] data)
    {
      using (var compressedStream = new MemoryStream(data))
      using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
      using (var resultStream = new MemoryStream())
      {
        zipStream.CopyTo(resultStream);
        return resultStream.ToArray();
      }
    }

    private static List<List<float>> GetValues(DataRow row)
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
    
      var numBytesPerMember = byte_values.Length / member_count;

      for (int i = 0; i < member_count; i++)
      {
        var floatValues = new float[member_length];
        Buffer.BlockCopy(byte_values, i * numBytesPerMember, floatValues, 0, numBytesPerMember);
        var values = new List<float>();
        values.AddRange(floatValues);
        rval.Add(values);
      }

      return rval;
    }
     
  }
}
