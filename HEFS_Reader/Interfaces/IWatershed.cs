using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFS_Reader.Interfaces
{
	public interface IWatershed
	{
		IList<IEnsemble> Locations { get; }
		Enumerations.Watersheds WatershedName { get; }
	}
}
