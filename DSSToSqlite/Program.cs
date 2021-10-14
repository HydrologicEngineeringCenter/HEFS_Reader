using Hec.Dss;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSSToSqlite
{
  class Program
  {
    static void Main(string[] args)
    {




      if( args.Length != 2)
      {
        Console.WriteLine("Usage: file.dss  file.db)");
        return;
      }
      var dssFileName = args[0];
      var sqliteFileName = args[1];
      if (File.Exists(sqliteFileName))
        throw new Exception("File allready exists " + sqliteFileName);

      using (DssReader r = new DssReader(dssFileName))
      {

        var catalog = r.GetCatalog();
        var uc = GetCollections(catalog);

        Console.WriteLine("catlog.count = "+catalog.Count);



      }


    }

    /// <summary>
    /// Get unique collection by parsing F part, removing collection component
    /// </summary>
    /// <param name="catalog"></param>
    /// <returns></returns>
    private static object GetCollections(DssPathCollection catalog)
    {
      var fparts = catalog.GetUniqueFParts();

      return null;

    }
  }
}
