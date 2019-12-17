
package hec.timeseries.ensemble;

import java.util.*;
import java.time.*;

public class RfcCsvFile
{
  public String FileName;
  private ArrayList<String> LocationNames;
  public final ArrayList<String> getLocationNames()
  {
    return LocationNames;
  }
  private void setLocationNames(ArrayList<String> value)
  {
    LocationNames = value;
  }

  private LocalDateTime[] TimeStamps;
  public final LocalDateTime[] getTimeStamps()
  {
    return TimeStamps;
  }
  private void setTimeStamps(LocalDateTime[] value)
  {
    TimeStamps = value;
  }

  private float[][] Data;

  /**
   index to start of each location in Data
   */
  private HashMap<String, Integer> locationStart = new HashMap<String, Integer>();
  /**
   index to end of each location in Data
   */
  private HashMap<String, Integer> locationEnd = new HashMap<String, Integer>();

  private String[] header;

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

  /**
   CSV file format from California Nevada River Forecast Center
   https://www.cnrfc.noaa.gov/

   First column is date/time

   @param fileName
   */
  public RfcCsvFile(String fileName)
  {
    this.FileName = fileName;
//C# TO JAVA CONVERTER TODO TASK: There is no equivalent to implicit typing in Java unless the Java 10 inferred typing option is selected:
    var rows = File.ReadAllLines(fileName);
    ParseHeader(rows[0]);
    ParseData(rows);
  }


  public final float[][] GetEnsemble(String locationName)
  {
    float[][] rval = null;
    tangible.RefObject<float[][]> tempRef_rval = new tangible.RefObject<float[][]>(rval);
    GetEnsemble(locationName, tempRef_rval);
    rval = tempRef_rval.argValue;
    return rval;
  }
  /**
   Returns 2-D array where each row is an ensemble member
   note: this is an axis swap from the CSV on disk

   @param locationName
   @param swapAxis when true rows represent time steps
   @return
   */

  public final void GetEnsemble(String locationName, RefObject<float[][]> ensemble)
  {
    GetEnsemble(locationName, ensemble, false);
  }

  //C# TO JAVA CONVERTER NOTE: Java does not support optional parameters. Overloaded method(s) are created above:
//ORIGINAL LINE: public void GetEnsemble(string locationName, ref float[,] ensemble, bool swapAxis = false)
  public final void GetEnsemble(String locationName, RefObject<float[][]> ensemble, boolean swapAxis)
  {
    int idx1 = locationStart.get(locationName);
    int idx2 = locationEnd.get(locationName);

    int memberCount = idx2 - idx1 + 1; // height
    int timeCount = getTimeStamps().length; // width


    if (swapAxis)
    {
      if (ensemble.argValue == null || (ensemble.argValue.length == 0 ? 0 : ensemble.argValue[0].length) != memberCount || ensemble.argValue.length != timeCount)
      {
        ensemble.argValue = new float[timeCount][memberCount];
      }
      for (int m = 0; m < memberCount; m++)
      {
        for (int t = 0; t < timeCount; t++)
        {
          ensemble.argValue[t][m] = Data[m + idx1][t];
        }
      }
    }
    else
    {

      if (ensemble.argValue == null || ensemble.argValue.length != memberCount || (ensemble.argValue.length == 0 ? 0 : ensemble.argValue[0].length) != timeCount)
      {
        ensemble.argValue = new float[memberCount][timeCount];
      }

      Buffer.BlockCopy(Data, idx1 * timeCount * (Float.SIZE / Byte.SIZE), ensemble.argValue, 0, memberCount * timeCount * (Float.SIZE / Byte.SIZE));
    }

  }

  /**
   Parse data swaping axis
   rows represent timesteps
   columns represent locations

   @param rows
   */
  private void ParseData(String[] rows)
  {
    int idx2 = FindLastRowIndex(rows);
    int idx1 = 2; // data starts after two header lines
    int rowCount = idx2 - idx1 + 1;
    int columnCount = header.length - 1; // date column will not be part of data
    setTimeStamps(new LocalDateTime[rowCount]);
    Data = new float[columnCount][rowCount]; // swap axis
    for (int rowIdx = 0; rowIdx < rowCount; rowIdx++)
    {
      String[] values = rows[rowIdx + idx1].split("[,]", -1);
      getTimeStamps()[rowIdx] = ParseDateTime(values[0]); // first column is DateTime
      for (int columnIdx = 0; columnIdx < columnCount; columnIdx++)
      {
        // if (columnIdx >= values.Length)
        //  Console.WriteLine("Error: was file truncated? " + FileName);
        float f = Float.parseFloat(values[columnIdx + 1]);
        Data[columnIdx][rowIdx] = f;
      }
    }
  }
  /**
   find last row of data.
   some files have empty lines at the bottom.

   @param rows
   @return
   */
  private int FindLastRowIndex(String[] rows)
  {
    for (int i = rows.length - 1; i > 0; i--)
    {
      if (!rows[i].trim().equals(""))
      {
        return i;
      }
    }
    return -1;
  }


  private void ParseHeader(String line)
  {

    header = line.split("[,]", -1);
    String currHeader = "";

    setLocationNames(new ArrayList<String>());
    //first data element in header is timezone.
    for (int i = 1; i < header.length; i++)
    {
      if (!currHeader.equals(header[i]))
      {
        currHeader = header[i];
        getLocationNames().add(currHeader);
        locationStart.put(currHeader, i - 1);
      }
      else
      {
        locationEnd.put(currHeader, i - 1);

      }
    }
  }

  public static LocalDateTime ParseDateTime(String dt)
  {
    return LocalDateTime.parse(dt);
   // String[] dateTime = dt.split("[ ]", -1);
    //String[] yyyymmdd = dateTime[0].split("[-]", -1);
    //String[] hhmmss = dateTime[1].split("[:]", -1);
    //LocalDateTime output = LocalDateTime.of(Integer.parseInt(yyyymmdd[0]), Integer.parseInt(yyyymmdd[1]), Integer.parseInt(yyyymmdd[2]), Integer.parseInt(hhmmss[0]), Integer.parseInt(hhmmss[1]), Integer.parseInt(hhmmss[2]));
    //return output;
  }
}

