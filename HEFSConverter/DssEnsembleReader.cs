using HEFS_Reader.Enumerations;
using HEFS_Reader.Implementations;
using HEFS_Reader.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFSConverter
{
  class DssEnsembleReader : HEFS_Reader.Interfaces.IEnsembleReader
  {

    internal static HEFS_Reader.Interfaces.IWatershedForecast Read(string dssFileName, Watersheds waterShed,
                                          DateTime startTime, DateTime endTime, out TimeSpan timeSpan)
    {
      List<HEFS_Reader.Interfaces.IEnsemble> ensembles = new List<HEFS_Reader.Interfaces.IEnsemble>();
      var watershed = new HEFS_Reader.Implementations.WatershedForecast(ensembles, waterShed);

      var sw = System.Diagnostics.Stopwatch.StartNew();
      using (var dss = new DSSIO.DSSReader(dssFileName))
      {
        var dssPaths = dss.GetCondensedPathNames(); // sorted
        if (dssPaths.Count == 0)
        {
          throw new Exception("Empty DSS catalog");
        }
        // /RUSSIANNAPA/APCC1/FLOW/01SEP2019/1HOUR/C:000002|T:0212019/
        List<List<float>> ensembleValues = new List<List<float>>();
        int size = dssPaths.Count;

        for (int i = 0; i < size; i++)
        {
          DSSIO.DSSPath path = dssPaths[i];
          string location = path.Bpart;
          DateTime issueDate = ParseIssueDate(path.Fpart);
          if (issueDate >= startTime && issueDate <= endTime
            && path.Apart.ToLower() == waterShed.ToString().ToLower())
          {
            var ts = dss.GetTimeSeries(path.FullPath);
            List<float> memberValues = new List<float>();
            memberValues.AddRange(Array.ConvertAll(ts.Values, item => (float)item));
            ensembleValues.Add(memberValues);
            if (i == size - 1 || dssPaths[i + 1].Bpart != location)
            {// package this ensemble
              Ensemble e = new Ensemble(location, issueDate, ensembleValues, ts.Times.ToList());
              ensembles.Add(e);
              // start building next ensemble
              ensembleValues = new List<List<float>>();
            }
          }
        }

        // args.location.ToString()
      }
      sw.Stop();
      timeSpan = sw.Elapsed;
      return watershed;
    }

    /// <summary>
    /// parse issue date from part F:
    /// C:000002|T:0212019
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static DateTime ParseIssueDate(string fPart)
    {
      string[] tokens = fPart.Split('|')[1].Split(':');
      int year = Convert.ToInt32(tokens[1].Substring(3));
      string sday = tokens[1].Substring(0, 3);
      int day = Convert.ToInt32(sday);
      DateTime issueDate = new DateTime(year, 1, 1).AddDays(day - 1).AddHours(12);
      return issueDate;
    }

    public IWatershedForecast Read(IHEFSReadArgs args)
    {
      throw new NotImplementedException();
    }

    //public IWatershedForecast Read(IHEFSReadArgs args)
    //{
    //  List<HEFS_Reader.Interfaces.IEnsemble> ensembles = new List<HEFS_Reader.Interfaces.IEnsemble>();
    //  var watershed = new HEFS_Reader.Implementations.WatershedForecast(ensembles, args.WatershedLocation);
    //  using (var dss = new DSSIO.DSSReader(args.Path))
    //  {
    //    var dssPaths = dss.GetCondensedPathNames(); // sorted
    //    if (dssPaths.Count == 0)
    //    {
    //      throw new Exception("Empty DSS catalog");
    //    }
    //    //need all locations => b being *
    //    //need all collection members who end in T: args.ForecastDate.DayOfYear + args.ForecastDate.Year
    //    // /RUSSIANNAPA/APCC1/FLOW/01SEP2019/1HOUR/C:000002|T:0212019/
    //    List<List<float>> ensembleValues = new List<List<float>>();
    //    //int size = dssPaths.Count;




    //    for (int i = 0; i < size; i++)
    //    {
    //      DSSIO.DSSPath path = dssPaths[i];
    //      string location = path.Bpart;
    //      DateTime issueDate = ParseIssueDate(path.Fpart);
    //      if (issueDate >= startTime && issueDate <= endTime
    //        && path.Apart.ToLower() == args.WatershedLocation.ToString().ToLower())
    //      {
    //        var ts = dss.GetTimeSeries(path.FullPath);
    //        List<float> memberValues = new List<float>();
    //        memberValues.AddRange(Array.ConvertAll(ts.Values, item => (float)item));
    //        ensembleValues.Add(memberValues);
    //        if (i == size - 1 || dssPaths[i + 1].Bpart != location)
    //        {// package this ensemble
    //          Ensemble e = new Ensemble(location, issueDate, ensembleValues, ts.Times.ToList());
    //          ensembles.Add(e);
    //          // start building next ensemble
    //          ensembleValues = new List<List<float>>();
    //        }
    //      }
    //    }

    //    // args.location.ToString()
    //  }
    //  return watershed;
    //}

    public ITimeSeriesOfEnsembleLocations ReadDataset(Watersheds watershed, DateTime start, DateTime end, string dssPath)
    {
      //create catalog here,
      // loop through start times until end time and call read appropriately.

      List<HEFS_Reader.Interfaces.IEnsemble> ensembles = new List<HEFS_Reader.Interfaces.IEnsemble>();
      WatershedForecast watershedForecast = new WatershedForecast(ensembles, watershed);
      TimeSeriesOfEnsembleLocations rval = new TimeSeriesOfEnsembleLocations();
      rval.timeSeriesOfEnsembleLocations.Add(watershedForecast);

      using (var dss = new DSSIO.DSSReader(dssPath))
      {
        var dssPaths = dss.GetCondensedPathNames(); // sorted
        if (dssPaths.Count == 0)
        {
          throw new Exception("Empty DSS catalog");
        }
        //need all locations => b being *
        //need all collection members who end in T: args.ForecastDate.DayOfYear + args.ForecastDate.Year
        // /RUSSIANNAPA/APCC1/FLOW/01SEP2019/1HOUR/C:000002|T:0212019/
        List<List<float>> ensembleValues = new List<List<float>>();
        int size = dssPaths.Count;
        for (int i = 0; i < size; i++)
        {
          DSSIO.DSSPath path = dssPaths[i];
          string location = path.Bpart;
          DateTime issueDate = ParseIssueDate(path.Fpart);
          if (issueDate >= start && issueDate <= end
            && path.Apart.ToLower() == watershed.ToString().ToLower())
          {
            var ts = dss.GetTimeSeries(path.FullPath);
            List<float> memberValues = new List<float>();
            memberValues.AddRange(Array.ConvertAll(ts.Values, item => (float)item));
            ensembleValues.Add(memberValues);
            if (i == size - 1 || dssPaths[i + 1].Bpart != location)
            {// package this ensemble
              Ensemble e = new Ensemble(location, issueDate, ensembleValues, ts.Times.ToList());
              ensembles.Add(e);
              // start building next ensemble
              ensembleValues = new List<List<float>>();
            }
          }
        }
      }
      return rval;
    }
  }
}
