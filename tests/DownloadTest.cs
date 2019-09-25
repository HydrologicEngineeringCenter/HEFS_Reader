using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests
{
    [TestClass]
    public class DownloadTest
    {
        [TestMethod]
        public void TestFetchData()
        {
            /////https://www.cnrfc.noaa.gov/csv/2019092312_RussianNapa_hefs_csv_hourly.zip
            var args = new HEFS_Reader.Implementations.HEFSRequestArgs();
            args.location = HEFS_Reader.Enumerations.Watersheds.RussianNapa;
            args.date = "2019092312";

            var d = new HEFS_Reader.Implementations.HEFS_Downloader();
            if( d.FetchData(args))
            {
               
                Console.WriteLine(d.Response);
                Assert.IsTrue(d.Response.StartsWith("GMT,NVRC1,NVRC1,NVRC1,NVRC1,NVRC1,NVRC1"));
            }

           }

        [TestMethod]
        public void TestgetDataForWatershedAndTimeRange()
        {
            var x = new HEFS_Reader.Implementations.TimeSeriesOfEnsembles();
            
            DateTime t2 = DateTime.Now.Date.AddDays(-1).AddHours(12);
            DateTime t1 = t2.AddDays(-3);
            IList<IList<HEFS_Reader.Interfaces.IEnsemble>> e = x.getDataForWatershedAndTimeRange(HEFS_Reader.Enumerations.Watersheds.RussianNapa, t1, t2);
            Assert.AreEqual(4, e.Count);


            Assert.IsTrue(e[0][0].getMembers()[0].getValues().Length > 0);
 
        }
    }
}
