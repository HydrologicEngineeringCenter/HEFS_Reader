using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFS_Reader.Interfaces
{
  public interface IHEFSReadArgs
  {
    DateTime ForecastDate { get; set; }
    Enumerations.Watersheds WatershedLocation { get; set; }
    string Path { get; set; }
  }
}
