using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using H5Assist;

namespace Hec.TimeSeries.Ensemble
{
  public static class HDF5Ensemble
  {
    public static string Path(params string[] items)
    {
      return String.Join(H5Reader.PathSeparator, items);
    }
    public static void Write(H5Writer h5w, Watershed watershed)
    {
      int chunkSize = 1;
      string root = Path(H5Reader.Root, "Watersheds", watershed.Name);

      long[] dtTicks = null;

      foreach (Location loc in watershed.Locations)
      {
        string watershedPath = Path(root, loc.Name);

        // For each forecast
        foreach (Forecast f in loc.Forecasts)
        {
          // I think this isn't unique... needs day/yr as well based on karls code.
          string ensemblePath = Path(watershedPath,
            f.IssueDate.Year.ToString() + "_" + f.IssueDate.DayOfYear.ToString());

          h5w.CreateGroup(ensemblePath);

          // 2 datasets under this group - times and values. Write out times as long tick counts,
          // and values as floats. Both will be column-striated for easy vertical access

          string valuePath = Path(ensemblePath, "Values");
          string timePath = Path(ensemblePath, "Times");

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
            int width = f.Ensemble.GetLength(1);
            float[] vals = new float[width];
            // Each ensemble member is a time-series of data, add a new column
            for (int ensembleMember = 0; ensembleMember < f.Ensemble.GetLength(0); ensembleMember++)
            {
              Buffer.BlockCopy(f.Ensemble, ensembleMember * width * sizeof(float), vals, 0, width * sizeof(float));
              h5w.AddRow(valueDset, vals);
            }
          }

        }
      }
    }


    public static void WriteParallel(H5Writer h5w, Watershed watershed, int desiredChunkSize)
    {
      string root = Path(H5Reader.Root, "Watersheds", watershed.Name);

      var tlDt = new ThreadLocal<long[]>(() => Array.Empty<long>());
      var tlValues = new ThreadLocal<float[]>(() => Array.Empty<float>());

      object grpLock = new object();

      foreach (Location loc in watershed.Locations)
      {
        string locationPath = Path(root, loc.Name);

        Parallel.ForEach(loc.Forecasts, f =>
        {
          string forecastPath = Path(locationPath,
            f.IssueDate.Year.ToString() + "_" + f.IssueDate.DayOfYear.ToString());

          // This isn't crashing, but it's causing crazy console output because of a race condition between
          // "Does it exist" and "Please create it" (afaict)
          lock (grpLock)
            h5w.CreateGroup(forecastPath);

          // 2 datasets under this group - times and values. Write out times as long tick counts,
          // and values as floats. Both will be column-striated for easy vertical access

          string valuePath = Path(forecastPath, "Values");
          string timePath = Path(forecastPath, "Times");

          // Based on Karl's example, all ensembles have to have the same DateTimes. Write it once.
          var dts = f.TimeStamps;
          long[] dtTicks = tlDt.Value;
          if (dtTicks == null || dtTicks.Length != dts.Length)
          {
            dtTicks = new long[dts.Length];
            tlDt.Value = dtTicks;
          }

          for (int i = 0; i < dts.Length; i++)
            dtTicks[i] = dts[i].Ticks;

          // Saves a lot of time in the hdf lib I think...
          h5w.WriteUncompressed(timePath, dtTicks);

          // Again, I think this is guaranteed since we're only writing one 'times' dataset
          int nColumns = dts.Length;

          // Use -1 to mean "all members in this ensemble"
          int numMembers = f.Ensemble.GetLength(0);
          int chunkSize = desiredChunkSize;
          if (chunkSize == -1)
            chunkSize = numMembers;

          float[] buf = tlValues.Value;
          if (buf == null || buf.Length != nColumns * chunkSize)
          {
            buf = new float[nColumns * chunkSize];
            tlValues.Value = buf;
          }

          // Was initially writing columns - I think rows are better. (1 row per ensemble).
          // This way we can chunk multiple rows at a time without changing access patterns,
          // and it lets us block-copy conveniently on reads.
          h5w.Create2dExtendibleDataset<float>(valuePath, chunkSize, nColumns);

          using (var valueDset = h5w.OpenDataset(valuePath))
          {
            h5w.SetExtent(valueDset, new long[] { numMembers, nColumns });

            // Each ensemble member is a time-series of data, add a new row...

            // Row-index within this chunk, resets whenever we write a chunk
            int relativeRow = 0;

            int[] chunks = new int[2];
            chunks[0] = 0; // Row-start, will change
            chunks[1] = 0; // Column-start, won't change
            for (int rowIndex = 0; rowIndex < numMembers; rowIndex++)
            {
              // Copy into our chunkbuffer
              int ensembleOffset = rowIndex * nColumns * sizeof(float);
              Buffer.BlockCopy(f.Ensemble, 0, buf, relativeRow * nColumns * sizeof(float), nColumns * sizeof(float));

              relativeRow++;

              // Are we done with this chunk?
              if (relativeRow == chunkSize)
              {
                // HDF5 is threadsafe. But when the compression happens internally, it locks and forces serial behavior.
                // This matters even if it's one row per chunk, since we can get a speedup by compressing externally
                h5w.WriteChunkDirect_Threadsafe(valueDset, chunks, buf);

                // Reset
                relativeRow = 0;
                chunks[0] += 1;
              }
              else if (rowIndex == numMembers - 1)
              {
                // We have some number of rows to write at the end... wipe the rest of the buffer 
                // so it compresses better and gets unzipped as zeroes?
                for (int i = relativeRow * nColumns; i < buf.Length; i++)
                  buf[i] = 0;

                h5w.WriteChunkDirect_Threadsafe(valueDset, chunks, buf);
              }
            }
          }

        });

      }
    }


    public static Watershed Read(H5Reader h5r, string watershedName)
    {
      string root = Path(H5Reader.Root, "Watersheds", watershedName);
      long[] dtTicks = null;
      float[,] data = null;

      Watershed retn = new Watershed(watershedName);

      var locationNames = h5r.GetGroupNames(root);
      foreach (var loc in locationNames)
      {
        var forecastNames = h5r.GetGroupNames(Path(root, loc));

        foreach (var forecastDate in forecastNames)
        {
          //Watersheds/EastSierra/BCAC1/2013_307
          string forecastPath = Path(root, loc, forecastDate);
          if (!TryParseIssueDate(forecastDate, out DateTime issueDate))
          {
            Console.WriteLine("ERROR IN HDF5 PATH: " + forecastPath);
            continue;
          }

          h5r.ReadDataset(Path(forecastPath, "Times"), ref dtTicks);
          h5r.ReadDataset(Path(forecastPath, "Values"), ref data);
          var _times = dtTicks.Select(t => new DateTime(t)).ToArray();
          retn.AddForecast(loc, issueDate, data, _times);
        }
      }

      return retn;
    }

    private static bool TryParseIssueDate(string forecastDate, out DateTime issueDate)
    {
      string[] split = forecastDate.Split('_');
      if (split.Length != 2)
      {
        issueDate = default(DateTime);
        return false;
      }
      int yr = int.Parse(split[0]);
      int day = int.Parse(split[1]);
      issueDate = new DateTime(yr, 1, 1);
      issueDate = issueDate.AddDays(day - 1);
      return true;
    }
  }
}
