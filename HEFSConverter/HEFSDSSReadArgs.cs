using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HEFS_Reader.Enumerations;

namespace HEFSConverter
{
  class HEFSDSSReadArgs : HEFS_Reader.Interfaces.IHEFSReadArgs
  {
    public DateTime ForecastDate { get; set; }
    public Watersheds WatershedLocation { get; set; }
    public string Path { get; set; }
  }
}
