using DSSIO;
using HEFS_Reader.Enumerations;
using HEFS_Reader.Implementations;
using HEFS_Reader.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HEFSConverter
{
  public class DssEnsembleReader : IEnsembleReader, ITimeable
  {
    private long _readTimeInMilliSeconds = 0;
    public long ReadTimeInMilliSeconds { get { return _readTimeInMilliSeconds; } }


    /// <summary>
    /// parse issue date from part F:
    /// C:000002|T:0212019
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static DateTime ParseIssueDate(string input)
    {
      int idx = input.IndexOf("T:");
      if (idx < 0)
        throw new Exception("Could not parse issue date from '" + input + "'");

      input = input.Substring(idx + 2);

      int year = Convert.ToInt32(input.Substring(3));
      string sday = input.Substring(0, 3);
      int day = Convert.ToInt32(sday);
      DateTime issueDate = new DateTime(year, 1, 1).AddDays(day - 1).AddHours(12);
      return issueDate;
    }

    public IWatershedForecast Read(IHEFSReadArgs args)
    {
      throw new NotImplementedException();
    }

    public ITimeSeriesOfEnsembleLocations ReadDataset(Watersheds watershed, DateTime start, DateTime end, string dssPath)
    {
      TimeSeriesOfEnsembleLocations rval = new TimeSeriesOfEnsembleLocations();

      using (DSSReader dss = new DSSReader(dssPath, DSSReader.MethodID.MESS_METHOD_GENERAL_ID, DSSReader.LevelID.MESS_LEVEL_NONE))
      {
        Console.WriteLine("Reading " + dssPath);
        DSSPathCollection rawDssPaths = dss.GetCatalog(); // sorted
        var dssPaths = rawDssPaths.OrderBy(a => a, new PathComparer()).ToArray(); // sorted
        int size = dssPaths.Length;
        if (size == 0)
        {
          throw new Exception("Empty DSS catalog");
        }

        // /RUSSIANNAPA/APCC1/FLOW/01SEP2019/1HOUR/C:000002|T:0212019/
        for (int i = 0; i < size; i++)
        {
          if (i % 100 == 0)
            Console.Write(".");

          DSSPath path = dssPaths[i];
          string location = path.Bpart;
          int memberidx = int.Parse(path.Fpart.Split('|')[0].Split(':').Last().TrimStart('0'));

          DateTime issueDate = ParseIssueDate(path.Fpart);

          if (issueDate >= start && issueDate <= end && path.Apart.ToLower() == watershed.ToString().ToLower())
          {
            var ts = dss.GetTimeSeries(path.PathWithoutDate);

            var em = new EnsembleMember(ts.Values.Select(d => (float)d).ToArray(), ts.Times);
            // rval.Forecasts.Insert(memberidx - 1, )
            rval.AddEnsembleMember(em, memberidx - 1, issueDate, location, watershed);
          }
         }
      }

      rval.SortByIssuanceDate();
      return rval;
    }
    
    public ITimeSeriesOfEnsembleLocations ReadDatasetFromProfiles(Watersheds watershed, DateTime start, DateTime end, string dssPath)
    {
      var st = Stopwatch.StartNew();
      TimeSeriesOfEnsembleLocations rval = new TimeSeriesOfEnsembleLocations();

      using (DSSReader dss = new DSSReader(dssPath, DSSReader.MethodID.MESS_METHOD_GENERAL_ID, DSSReader.LevelID.MESS_LEVEL_NONE))
      {
        Console.WriteLine("Reading" + dssPath);
        DSSPathCollection rawDssPaths = dss.GetCatalog(); // sorted
        var dssPaths = rawDssPaths.OrderBy(a => a, new PathComparer()).ToArray(); // sorted
        int size = dssPaths.Length;
        if (size == 0)
        {
          throw new Exception("Empty DSS catalog");
        }

        // /RUSSIANNAPA/APCC1/FLOW/01SEP2019/1HOUR/|T:0212019/
        for (int i = 0; i < size; i++)
        {
          if (i % 100 == 0)
            Console.Write(".");

          DSSPath path = dssPaths[i];
          string location = path.Bpart;

          DateTime issueDate = ParseIssueDate(path.Fpart);

          if (issueDate >= start && issueDate <= end
            && path.Apart.ToLower() == watershed.ToString().ToLower())
          {
            var ts = dss.GetTimeSeriesProfile(path);
            var columnNames = ts.ColumnValues;

            for (int m = 0; m < columnNames.Length; m++)
            {
              int sz = ts.Values.Length;
              var memberValues = new float[sz];
             
              for (int row = 0; row < sz; row++)
              {
                memberValues[row] = ((float)ts.Values[row][m]);
              }

              var em = new EnsembleMember(memberValues, ts.Times);
              rval.AddEnsembleMember(em, m , issueDate, location, watershed);
            }

          }
        }
      }

      rval.SortByIssuanceDate();
      st.Stop();
      Console.WriteLine();
      _readTimeInMilliSeconds = st.ElapsedMilliseconds;
      return rval;
    }
    
    private static List<DateTime> GetTimes(DateTime t, int count)
    {
      var rval = new List<DateTime>(count);
      for (int i = 0; i < count; i++)
      {
        rval.Add(t);
        t = t.AddHours(1); // hardcode hourly
      }
      return rval;
    }


    class PathComparer : IComparer<DSSPath>
    {
      public int Compare(DSSPath x, DSSPath y)
      {

        return string.Compare(CollectionSortable(x), CollectionSortable(y));
      }

      //            string previousT = dssPaths[0].Fpart.Split('|').Last().Split(':').Last();
      //          string previousLoc = dssPaths[0].Bpart;
      // /RUSSIANNAPA/APCC1/FLOW/01SEP2019/1HOUR/C:000002|T:0212019 

      private string CollectionSortable(DSSPath x)
      {
        string rval = x.Apart + x.Bpart + x.Cpart + x.SortableDPart + x.Epart;

        if (!x.Fpart.StartsWith("C:"))
          return rval;

        var tokens = x.Fpart.Split('|');
        if (tokens.Length != 2)
          return x.PathWithoutDate;
        rval += tokens[1].Split(':')[1] + tokens[0].Split(':')[1];
        return rval;
      }
    }

  }
}
