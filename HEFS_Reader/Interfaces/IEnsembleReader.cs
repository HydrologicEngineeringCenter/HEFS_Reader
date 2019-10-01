using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFS_Reader.Interfaces
{
    public interface IEnsembleReader
    {
        IWatershed Read(Implementations.HEFSRequestArgs args);
    }
}
