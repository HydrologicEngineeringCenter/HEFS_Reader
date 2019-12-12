using HEFS_Reader.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFS_Downloader
{

  class Program
  {
    static string cacheDir = @"C:\Temp\hefs_cache";

    static void Main(string[] args)
    {
      DateTime startTime = new DateTime(2013, 11, 1, 12, 0, 0);
      DateTime endTime = DateTime.Now.Date.AddDays(-1).AddHours(12);

      System.IO.Directory.CreateDirectory(cacheDir);
      var watershedNames = new string[] { "RussianNapa", "EastSierra", "FeatherYuba" };
      DateTime t1 = new DateTime(2013, 11, 1, 12, 0, 0);
      DateTime t2 = new DateTime(2019, 11, 18, 12, 0,0);

      var t = t1;
      while (t <= t2)
      {
        foreach (var w in watershedNames)
        {
          HEFS_WebReader.Read(w, t, cacheDir);
        }
        t = t.AddDays(1);
      }

    }
  }
}
