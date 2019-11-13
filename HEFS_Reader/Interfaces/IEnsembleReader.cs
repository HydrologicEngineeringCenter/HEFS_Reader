using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HEFS_Reader.Implementations;

namespace HEFS_Reader.Interfaces
{
  public interface IEnsembleReader
  {
    /// <summary>
    /// This is where the magic happens for reading. We need to be able to access these ensemble datasets with lightning speed!
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    TimeSeriesOfEnsembleLocations ReadDataset(Enumerations.Watersheds watershed, DateTime start, DateTime end, String Path);

  }
}
