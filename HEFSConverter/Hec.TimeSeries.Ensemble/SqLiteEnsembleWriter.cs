using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hec.TimeSeries.Ensemble
{
  /// <summary>
  /// SqLiteEnsembleWriter opens a connection and transaction
  /// to write multiple ensemble blob records as a single transaction
  /// </summary>
  public class SqLiteEnsembleWriter:IDisposable
  {

    public static string GetCreateTableSQL(string tableName)
    {
      return "CREATE TABLE IF NOT EXISTS " + tableName
      + " ( id integer not null primary key,"
      + "    issue_date datetime, "
      + "   watershed NVARCHAR(100) ,"
      + "   location_name NVARCHAR(100) ,"
      + "   timeseries_start_date datetime ,"
      + "   member_length integer    ,"
      + "   member_count integer    ,"
      + "   compressed integer    ,"
      + "  byte_value_array BLOB NULL )";
    }

    static string GetConnectionString(string filename)
    {
      return "Data Source=" + filename + ";Synchronous=Off;Pooling=True;Journal Mode=Off";
    }

    SQLiteConnection connection = null;
    SQLiteTransaction transaction = null;
    SQLiteCommand cmd = null;

    internal int MaxID()
    {
      var cmd1 = connection.CreateCommand();
      cmd1.CommandText = "select count(*), max(id) from timeseries_blob";
      var reader = cmd1.ExecuteReader();
      reader.Read();
      int count = reader.GetInt32(0);
      int maxIndex = 0;
      if (count != 0)
      {
        maxIndex = reader.GetInt32(1);
      }


      reader.Close();
      return maxIndex;
      
    }

    public SqLiteEnsembleWriter(string filename)
    {
      connection = new SQLiteConnection(GetConnectionString(filename));
      connection.Open();

      var cmd1 = connection.CreateCommand();
      cmd1.CommandText = GetCreateTableSQL("timeseries_blob");
      cmd1.ExecuteNonQuery();
      
      cmd = connection.CreateCommand();
      cmd.CommandText = "INSERT INTO [main].[sqlite_default_schema].[timeseries_blob] ([id], [issue_date], [watershed], [location_name], "+
 " [timeseries_start_date], [member_length], [member_count], [compressed], [byte_value_array]) VALUES "+
"(@param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8, @param9)";
      cmd.Parameters.Add("param1", System.Data.DbType.Int32);
      cmd.Parameters.Add("param2", System.Data.DbType.DateTime);
      cmd.Parameters.Add("param3", System.Data.DbType.String);
      cmd.Parameters.Add("param4", System.Data.DbType.String);
      cmd.Parameters.Add("param5", System.Data.DbType.DateTime);
      cmd.Parameters.Add("param6", System.Data.DbType.Int32);
      cmd.Parameters.Add("param7", System.Data.DbType.Int32);
      cmd.Parameters.Add("param8", System.Data.DbType.Boolean);
      cmd.Parameters.Add("param9", System.Data.DbType.Binary);


      transaction = connection.BeginTransaction();

    }

    public void InsertEnsemble(int id, DateTime issue_date, string watershed, string location_name,
      DateTime timeseries_start_date,int member_length, int member_count,bool compressed,
      byte[] byte_value_array)
    {
      cmd.Parameters[0].Value = id;
      cmd.Parameters[1].Value = issue_date;
      cmd.Parameters[2].Value = watershed;
      cmd.Parameters[3].Value = location_name;
      cmd.Parameters[4].Value = timeseries_start_date;
      cmd.Parameters[5].Value = member_length;
      cmd.Parameters[6].Value = member_count;
      cmd.Parameters[7].Value = compressed;
      cmd.Parameters[8].Value = byte_value_array;

      cmd.ExecuteNonQuery();
      /*
      row["id"] = index;
      row["issue_date"] = f.IssueDate;
      row["watershed"] = watershed.Name;
      row["location_name"] = loc.Name;
      row["timeseries_start_date"] = timeseries_start_date;
      row["member_length"] = f.Ensemble.GetLength(1);
      row["member_count"] = f.Ensemble.GetLength(0);
      row["compressed"] = compress ? 1 : 0;
      row["byte_value_array"] = ConvertToBytes(f.Ensemble, compress, ref uncompressed);
      */
    }


    public void Dispose()
    {
      transaction.Commit();
      connection.Close();
    }

  }
}
