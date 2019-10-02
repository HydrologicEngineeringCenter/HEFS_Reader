using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFS_Reader.Interfaces
{
    public interface IEnsembleReader
    {
        /// <summary>
		/// This is where the magic happens for reading. We need to be able to access these ensemble datasets with lightning speed!
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		IWatershed Read(Implementations.HEFSRequestArgs args);
    }
}
