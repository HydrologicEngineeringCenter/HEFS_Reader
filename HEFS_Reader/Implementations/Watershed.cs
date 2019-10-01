using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEFS_Reader.Implementations
{
	public class Watershed : Interfaces.IWatershed
	{
		private IList<Interfaces.IEnsemble> _ensembles;
		private Enumerations.Watersheds _watershedName;
		public IList<Interfaces.IEnsemble> Locations
		{
			get
			{
				return _ensembles;
			}
		}
		public Enumerations.Watersheds WatershedName
		{
			get
			{
				return _watershedName;
			}
		}
		public Watershed(IList<Interfaces.IEnsemble> ensembles, Enumerations.Watersheds watershedName)
		{
			_ensembles = ensembles;
			_watershedName = watershedName;
		}
	}
}
