using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFS_Reader.Interfaces
{
    public interface IHEFS_DataServiceProvider
    {
        string Response { get; }
        string CacheDirectory { get; }
        bool FetchData(Implementations.HEFSRequestArgs args);
    }
}
