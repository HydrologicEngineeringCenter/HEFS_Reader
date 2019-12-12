using System;
using System.IO;
using System.Net;

namespace HEFS_Reader.Implementations
{
  public class HEFS_WebReader
  {

    public static void Read(string watershedName, DateTime ForecastDate, string outputDir)
    {
      string _rootUrl = "https://www.cnrfc.noaa.gov/csv/";
      //https://www.cnrfc.noaa.gov/csv/2019092312_RussianNapa_hefs_csv_hourly.zip
      string webrequest = _rootUrl;

      string fileName = ForecastDate.ToString("yyyyMMddhh") + "_";
      fileName += watershedName;
      fileName += "_hefs_csv_hourly";
      webrequest += fileName + ".zip";

      string zipFileName = Path.Combine(outputDir, fileName + ".zip");
      string csvFileName = Path.Combine(outputDir, fileName + ".csv");
      if (File.Exists(csvFileName))
      {
        Console.WriteLine("Found " + csvFileName + " in cache.  skipping...");
        return;
      }

      if (!File.Exists(zipFileName))
      {
        try
        {
          Console.WriteLine("downloading " + zipFileName);
          GetFile(webrequest, zipFileName);
        }
        catch (System.Net.WebException ex)
        {
          Console.WriteLine(ex.Message);
          Console.WriteLine("404, " + zipFileName + " not found ");
          return;
        }
      }
      Reclamation.Core.ZipFileUtility.UnzipFile(zipFileName, csvFileName);
    }
  



    /// <summary>
    /// from https://stackoverflow.com/questions/137285/what-is-the-best-way-to-read-getresponsestream
    /// </summary>
    static void GetFile(string url, string outputFilename)
    {
      HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
      httpRequest.Method = "GET";

      // if the URI doesn't exist, an exception will be thrown here...
      using (HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse())
      {
        using (Stream responseStream = httpResponse.GetResponseStream())
        {
          using (FileStream localFileStream =
              new FileStream(outputFilename, FileMode.Create))
          {
            var buffer = new byte[4096];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
            {
              totalBytesRead += bytesRead;
              localFileStream.Write(buffer, 0, bytesRead);
            }
          }
        }
      }
    }
  }
}