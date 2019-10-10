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

        static string tableName = "timeseries_hourly";
        public static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";


    internal static TimeSpan Write(SQLiteServer server, ITimeSeriesOfEnsembleLocations watersheds,
      bool compress =false)
    {
      Stopwatch sw = Stopwatch.StartNew();
      int index = 0;
      var timeSeriesTable = GetHourlyBlobTable(server);

      var newRowLock = new object();

      foreach (IWatershedForecast watershed in watersheds.timeSeriesOfEnsembleLocations)
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
            row["member_length"] = e.Members[0].Values.Length;
            row["member_count"] = e.Members.Count;
            row["compressed"] = compress ?1 :0;
            row["byte_value_array"] = ConvertToBytes(e.Members, compress);

         // lock (newRowLock)
          //{
            timeSeriesTable.Rows.Add(row); // create rows in separate loop first
         // }
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
      var uncompressed = new byte[numBytesPerMember * ensembleMembers.Count  ];

      for (int i = 0; i < ensembleMembers.Count; i++)
      {
        values = ensembleMembers[i].Values;
        Buffer.BlockCopy(values, 0, uncompressed, i*numBytesPerMember, numBytesPerMember);
      }

      if (!compress)
        return uncompressed;
      var compressed =  Compress(uncompressed);
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
        using (var gs = new GZipStream(mso,  mode))
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
        static DataTable GetHourlyBlobTable(Reclamation.Core.BasicDBServer server)
        {
          

            if (!server.TableExists(tableName))
            {
                string sql = "CREATE TABLE " + tableName
                + " ( id integer not null primary key,"
                +"    issue_date datetime, "
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
