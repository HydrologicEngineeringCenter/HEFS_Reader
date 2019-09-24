using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests
{
    [TestClass]
    public class DownloadTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            /////https://www.cnrfc.noaa.gov/csv/2019092312_RussianNapa_hefs_csv_hourly.zip
            var args = new HEFS_Reader.Implementations.HEFSRequestArgs();
            args.location = "RussianNapa";
            args.date = "201909231";



           }
    }
}
