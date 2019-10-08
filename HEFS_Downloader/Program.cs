using HEFS_Reader.Enumerations;
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

            var provider = new HEFS_Reader.Implementations.HEFS_WebReader();
            System.IO.Directory.CreateDirectory(cacheDir);
            
            var waterShedData =
            provider.ReadDataset(Watersheds.RussianNapa, startTime, endTime, cacheDir);

        }
    }
}
