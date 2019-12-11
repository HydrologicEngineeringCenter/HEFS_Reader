using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hec.TimeSeries.Ensemble
{
  public class RfcCsvFile
  {
    public List<string> LocationNames { get; private set; }

    public DateTime[] TimeStamps { get; private set; }

    private float[,] Data;

    /// <summary>
    /// index to start of each location in Data
    /// </summary>
    Dictionary<string, int> locationStart = new Dictionary<string, int>();
    /// <summary>
    /// index to end of each location in Data
    /// </summary>
    Dictionary<string, int> locationEnd = new Dictionary<string, int>();
    
    private string[] header;

    // example:
    /*
     GMT,PLLC1,PLLC1,PLLC1,PLLC1,PLLC1,PLLC1,PLLC1,PLLC1
     ,QINE,QINE,QINE,QINE,QINE,QINE,QINE,QINE,QINE,QINE,
     2015-03-17 12:00:00,1.0728949,1.0728949,1.0728949,1
     2015-03-17 13:00:00,1.1079977,1.0526596,1.05326,1.0
     2015-03-17 14:00:00,1.1431005,1.0323889,1.033625,1.
     2015-03-17 15:00:00,1.1782385,1.0121536,1.01399,1.0
     2015-03-17 16:00:00,1.2133415,0.9919184,0.9943551,0
     2015-03-17 17:00:00,1.2484442,0.9716478,0.9747201,0
     2015-03-17 18:00:00,1.2835469,0.9514125,0.9550852,0
     2015-03-17 19:00:00,1.2741178,0.9471394,0.9483401,0
     2015-03-17 20:00:00,1.2646536,0.942831,0.941595,0.9
     2015-03-17 21:00:00,1.2552245,0.9385579,0.9348852,0
     2015-03-17 22:00:00,1.2457602,0.9342495,0.9281401,0
     2015-03-17 23:00:00,1.2363312,0.92997646,0.921395,0
     2015-03-18 00:00:00,1.226867,0.92566806,0.9146499,0
     2015-03-18 01:00:00,1.2062078,0.928246,0.9163803,0.
     2015-03-18 02:00:00,1.1855487,0.9307887,0.9181461,0
     2015-03-18 03:00:00,1.1648897,0.93336666,0.91987646
     2015-03-18 04:00:00,1.1441953,0.93590933,0.9216069,
     2015-03-18 05:00:00,1.1235362,0.93845195,0.9233726,
     */

    /// <summary>
    /// CSV file format from California Nevada River Forecast Center
    /// https://www.cnrfc.noaa.gov/
    /// 
    /// First column is date/time
    /// </summary>
    /// <param name="fileName"></param>
    public RfcCsvFile(string fileName)
    {
      var rows = File.ReadAllLines(fileName);
      ParseHeader(rows[0]);
      ParseData(rows);
    }

    /// <summary>
    /// Returns 2-D array where each row is an ensemble member
    /// note: this is an axis swap from the CSV on disk
    /// </summary>
    /// <param name="locationName"></param>
    /// <returns></returns>
    public float[,] GetEnsemble(string locationName)
    {
      int idx1 = locationStart[locationName];
      int idx2 = locationEnd[locationName];

      int memberCount = idx2 - idx1 + 1;
      int timeCount = TimeStamps.Length;
      float[,] rval = new float[memberCount, timeCount];

      // TO DO... block copy
      for (int m = 0; m < memberCount ; m++)
      {
        for (int t = 0; t < timeCount; t++)
        {
          rval[m, t] = Data[m,t];
        }
      }

      return rval;
    }

    /// <summary>
    /// Parse data swaping axis
    /// rows represent timesteps
    /// columns represent locations
    /// </summary>
    /// <param name="rows"></param>
    private void ParseData(string[] rows)
    {
      int idx2 = FindLastRowIndex(rows);
      int idx1 = 2; // data starts after two header lines
      int rowCount = idx2 - idx1 + 1;
      int columnCount = header.Length - 1; // date column will not be part of data
      TimeStamps = new DateTime[rowCount];
      Data = new float[columnCount,rowCount]; // swap axis
      for (int rowIdx = 0; rowIdx < rowCount; rowIdx++)
      {
        string[] values = rows[rowIdx+idx1].Split(',');
        TimeStamps[rowIdx] = ParseDateTime(values[0]); // first column is DateTime
        for (int columnIdx = 0; columnIdx < columnCount; columnIdx++)
        {
          var f = float.Parse(values[columnIdx + 1]);
          Data[columnIdx,rowIdx] = f;
        }
      }
    }
    /// <summary>
    /// find last row of data.
    /// some files have empty lines at the bottom.
    /// </summary>
    /// <param name="rows"></param>
    /// <returns></returns>
    private int FindLastRowIndex(string[] rows)
    {
      for (int i = rows.Length-1; i>0;  i--)
      {
        if (rows[i].Trim() != "")
          return i;
      }
      return -1;
    }


    private void ParseHeader(string line)
    {
     
      header = line.Split(',');
      string currHeader = "";
    
      LocationNames = new List<string>();
      //first data element in header is timezone.
      for (int i = 1; i < header.Length; i++)
      {
        if (!currHeader.Equals(header[i]))
        {
          currHeader = header[i];
          LocationNames.Add(currHeader);
          locationStart[currHeader]=i-1;
        }
        else
        {
          locationEnd[currHeader] = i-1;

        }
      }
    }

    public static DateTime ParseDateTime(string dt)
    {
      return DateTime.Parse(dt);
      string[] dateTime = dt.Split(' ');
      string[] yyyymmdd = dateTime[0].Split('-');
      string[] hhmmss = dateTime[1].Split(':');
      DateTime output = new DateTime(int.Parse(yyyymmdd[0]), int.Parse(yyyymmdd[1]), int.Parse(yyyymmdd[2]), int.Parse(hhmmss[0]), int.Parse(hhmmss[1]), int.Parse(hhmmss[2]));
      return output;
    }
  }
}
