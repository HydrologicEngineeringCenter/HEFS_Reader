using Hec.Dss;
using Hec.TimeSeries.Ensemble;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace espf2dss
{
   class Program
   {
      static void Main(string[] args)
      {
         var w = new DssWriter(@"c:\project\dss-file-collection\fpart-convention\dss-convention-part-a.dss");
         
        RfcEspfCsv csv = new RfcEspfCsv(@"C:\project\dss-file-collection\fpart-convention\2021-12-08_1956_GMT-HCDI1W_SQIN.ESPF10.csv");

         var dates = csv.TimeStamps;
         string dateFMT = "yyyyMMdd-hhmm";
         string[] Years = csv.ColumnNames.Skip(1).ToArray();

         for (int i = 0; i < Years.Length; i++) 
         {
            ///   A/B/FLOW//6Hour/FPART
            var t = csv.TimeStamps[0];
            //  //HELLS CANYON-DAM/FLOW-IN/01Nov2021 - 01May2023/6HOUR/C:001981|T:20211126-1800|V:20211126-1906|NWRFC 180-DAY VIA WEB Scrape/
            string F = "C:" + Years[i].PadLeft(6, '0')
                  + "|T:" + t.ToString(dateFMT)
                  + "|V:" + csv.IssueDate.ToString(dateFMT) + "|"+csv.RfcFile + " "+csv.Configuration;


            var path = "//HELLS CANYON-DAM/FLOW-IN//1Hour/" + F + "/";
            Hec.Dss.TimeSeries timeseries = new Hec.Dss.TimeSeries
            {
               Values = csv[i],
               Units = csv.Units,
               DataType = "INST-VAL",
               Path = new DssPath(path),
               StartDateTime = t
            };

            w.Write(timeseries, true);
         }
      }
   }
}
