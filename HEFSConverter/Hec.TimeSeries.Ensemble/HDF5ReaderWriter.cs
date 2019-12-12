using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using H5Assist;

namespace Hec.TimeSeries.Ensemble
{
  public static class HDF5ReaderWriter
  {
    public static string MakePath(params string[] items)
    {
      return String.Join(H5Reader.PathSeparator,items);
    }
    public static void Write(H5Writer h5w, Watershed watershed)
    {
      int chunkSize = 1;
      string root = MakePath(H5Reader.Root, "Watersheds", watershed.Name);

      long[] dtTicks = null;

      foreach (Location loc in watershed.Locations)
      {
        string watershedPath = MakePath(root, loc.Name);

        // For each forecast
        foreach (Forecast f in loc.Forecasts)
        {
          // I think this isn't unique... needs day/yr as well based on karls code.
          string ensemblePath =MakePath(watershedPath,loc.Name, 
            f.IssueDate.Year.ToString() + "_" + f.IssueDate.DayOfYear.ToString());

          h5w.CreateGroup(ensemblePath);

          // 2 datasets under this group - times and values. Write out times as long tick counts,
          // and values as floats. Both will be column-striated for easy vertical access

          string valuePath = MakePath(ensemblePath ,"Values");
          string timePath = MakePath(ensemblePath , "Times");

          // Based on Karl's example, all ensembles have to have the same DateTimes. Write it once.
          var dts = f.TimeStamps;
          if (dtTicks == null || dtTicks.Length != dts.Length)
            dtTicks = new long[dts.Length];

          for (int i = 0; i < dts.Length; i++)
            dtTicks[i] = dts[i].Ticks;

          h5w.WriteArray(timePath, dtTicks);

          // Again, I think this is guaranteed since we're only writing one 'times' dataset
          int firstSize = dts.Length;

          // Was initially writing columns - I think rows are better. (1 row per ensemble).
          // This way we can chunk multiple rows at a time without changing access patterns,
          // and it lets us block-copy conveniently on reads.
          h5w.Create2dExtendibleDataset<float>(valuePath, chunkSize, firstSize);

          using (var valueDset = h5w.OpenDataset(valuePath))
          {
            float[] vals = new float[f.Ensemble.GetLength(1)]; 
            // Each ensemble member is a time-series of data, add a new column
            for (int ensembleMember = 0; ensembleMember < f.Ensemble.GetLength(0); ensembleMember++)
            {
              Buffer.BlockCopy(f.Ensemble, ensembleMember, vals, 0, vals.Length * sizeof(float));
              h5w.AddRow(valueDset, vals);
            }
          }

        }
      }
    }


    //public static void WriteParallel(H5Writer h5w, Watershed watersheds, int desiredChunkSize)
    //{
    //  string root = H5Reader.Root + "Watersheds";

    //  var tlDt = new ThreadLocal<long[]>(() => Array.Empty<long>());
    //  var tlValues = new ThreadLocal<float[]>(() => Array.Empty<float>());

    //  object grpLock = new object();

    //  // For each [outer] watershed
    //  foreach (WatershedForecast watershed in watersheds.Forecasts)
    //  {
    //    string watershedPath = root + H5Reader.PathSeparator + watershed.WatershedName;

    //    Parallel.ForEach(watershed.Locations, e =>
    //    {
    //      // For each location

    //      // Needs day/yr as well based on karls code.
    //      string ensemblePath = watershedPath + H5Reader.PathSeparator + e.LocationName + H5Reader.PathSeparator +
    //        e.IssueDate.Year.ToString() + "_" + e.IssueDate.DayOfYear.ToString();

    //      // This isn't crashing, but it's causing crazy console output because of a race condition between
    //      // "Does it exist" and "Please create it" (afaict)
    //      lock (grpLock)
    //        h5w.CreateGroup(ensemblePath);

    //      // 2 datasets under this group - times and values. Write out times as long tick counts,
    //      // and values as floats. Both will be column-striated for easy vertical access

    //      string valuePath = ensemblePath + H5Reader.PathSeparator + "Values";
    //      string timePath = ensemblePath + H5Reader.PathSeparator + "Times";

    //      // Based on Karl's example, all ensembles have to have the same DateTimes. Write it once.
    //      var dts = e.Members.First().Times;
    //      long[] dtTicks = tlDt.Value;
    //      if (dtTicks == null || dtTicks.Length != dts.Length)
    //      {
    //        dtTicks = new long[dts.Length];
    //        tlDt.Value = dtTicks;
    //      }

    //      for (int i = 0; i < dts.Length; i++)
    //        dtTicks[i] = dts[i].Ticks;

    //      // Saves a lot of time in the hdf lib I think...
    //      h5w.WriteUncompressed(timePath, dtTicks);

    //      // Again, I think this is guaranteed since we're only writing one 'times' dataset
    //      int nColumns = dts.Length;

    //      // Use -1 to mean "all members in this ensemble"
    //      int chunkSize = desiredChunkSize;
    //      if (chunkSize == -1)
    //        chunkSize = e.Members.Count;

    //      float[] buf = tlValues.Value;
    //      if (buf == null || buf.Length != nColumns * chunkSize)
    //      {
    //        buf = new float[nColumns * chunkSize];
    //        tlValues.Value = buf;
    //      }

    //      // Was initially writing columns - I think rows are better. (1 row per ensemble).
    //      // This way we can chunk multiple rows at a time without changing access patterns,
    //      // and it lets us block-copy conveniently on reads.
    //      h5w.Create2dExtendibleDataset<float>(valuePath, chunkSize, nColumns);

    //      using (var valueDset = h5w.OpenDataset(valuePath))
    //      {
    //        h5w.SetExtent(valueDset, new long[] { e.Members.Count, nColumns });

    //        // Each ensemble member is a time-series of data, add a new row...

    //        // Row-index within this chunk, resets whenever we write a chunk
    //        int relativeRow = 0;

    //        int[] chunks = new int[2];
    //        chunks[0] = 0; // Row-start, will change
    //        chunks[1] = 0; // Column-start, won't change

    //        for (int ensembleMember = 0; ensembleMember < e.Members.Count; ensembleMember++)
    //        {
    //          EnsembleMember m = e.Members[ensembleMember];
    //          float[] vals = m.Values;

    //          // Copy into our chunkbuffer
    //          Buffer.BlockCopy(vals, 0, buf, relativeRow * nColumns * sizeof(float), nColumns * sizeof(float));

    //          relativeRow++;

    //          // Are we done with this chunk?
    //          if (relativeRow == chunkSize)
    //          {
    //            // HDF5 is threadsafe. But when the compression happens internally, it locks and forces serial behavior.
    //            // This matters even if it's one row per chunk, since we can get a speedup by compressing externally
    //            h5w.WriteChunkDirect_Threadsafe(valueDset, chunks, buf);

    //            // Reset
    //            relativeRow = 0;
    //            chunks[0] += 1;
    //          }
    //          else if (ensembleMember == e.Members.Count - 1)
    //          {
    //            // We have some number of rows to write at the end... wipe the rest of the buffer 
    //            // so it compresses better and gets unzipped as zeroes?
    //            for (int i = relativeRow * nColumns; i < buf.Length; i++)
    //              buf[i] = 0;

    //            h5w.WriteChunkDirect_Threadsafe(valueDset, chunks, buf);
    //          }
    //        }
    //      }

    //    });

    //  }
    //}




    //public static TimeSeriesOfEnsembleLocations Read(H5Reader h5r)
    //{
    //  string root = H5Reader.Root + "Watersheds";

    //  long[] dtTicks = null;
    //  float[,] data = null;

    //  TimeSeriesOfEnsembleLocations retn = new TimeSeriesOfEnsembleLocations();

    //  // For each [outer] watershed
    //  var shedGroups = h5r.GetGroupNames(root);
    //  foreach (var group in shedGroups)
    //  {
    //    // "RussianNapa"
    //    string wshedName = group;

    //    // Because for some reason this is an enum here
    //    List<Ensemble> ensembles = new List<Ensemble>();

    //    // Issue date added afterwards - TODO
    //    WatershedForecast watershedForecast = new WatershedForecast(ensembles, HEFS_Reader.Enumerations.Watersheds.RussianNapa, DateTime.MinValue);
        
    //    // For each location in this watershed
    //    string fullWshedPath = root + H5Reader.PathSeparator + group;
    //    var locGroups = h5r.GetGroupNames(fullWshedPath);

    //    foreach (var location in locGroups)
    //    {
    //      // location: "APCC1"
    //      // FullLocation: "/Watersheds/RussianNapa/APCC1"
    //      string fullLocation = fullWshedPath + H5Reader.PathSeparator + location;

    //      // Each location has a bunch of 1-day folders
    //      var dayGroups = h5r.GetGroupNames(fullLocation);

    //      foreach (var yrDay in dayGroups)
    //      {
    //        // yrDay: "2013_305"
    //        string fullDay = fullLocation + H5Reader.PathSeparator + yrDay;

    //        // Parse out the time
    //        string[] split = yrDay.Split('_');
    //        if(split.Length != 2)
    //        {
    //          Console.WriteLine("ERROR IN HDF5 PATH: " + fullDay);
    //          continue;
    //        }

    //        int yr = int.Parse(split[0]);
    //        int day = int.Parse(split[1]);


    //        string valuePath = fullDay + H5Reader.PathSeparator + "Values";
    //        string timePath = fullDay + H5Reader.PathSeparator + "Times";

    //        h5r.ReadDataset(timePath, ref dtTicks);
    //        h5r.ReadDataset(valuePath, ref data);

    //        DateTime issueDate = new DateTime(yr, 1, 1);
    //        issueDate = issueDate.AddDays(day - 1);

    //        Ensemble e = new Ensemble(location, issueDate, data, dtTicks);
    //        ensembles.Add(e);
    //      }
    //    }

    //    retn.Forecasts.Add(watershedForecast);
    //  }

    //  return retn;
    //}


  }
}
