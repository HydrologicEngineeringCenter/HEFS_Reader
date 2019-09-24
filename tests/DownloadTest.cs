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
            args.date = "2019092312";

            var d = new HEFS_Reader.Implementations.HEFS_Downloader();
            if( d.FetchData(args))
            {
               
                Console.WriteLine(d.Response);
                Assert.IsTrue(d.Response.StartsWith("GMT,NVRC1,NVRC1,NVRC1,NVRC1,NVRC1,NVRC1"));
            }

           }
    }
}
