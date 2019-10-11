using HEFS_Reader.Interfaces;
using System.Collections.Generic;

namespace HEFS_Reader.Implementations
{
	public class WatershedForecast : Interfaces.IWatershedForecast
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
		public WatershedForecast(IList<Interfaces.IEnsemble> ensembles, Enumerations.Watersheds watershedName)
		{
			_ensembles = ensembles;
			_watershedName = watershedName;
		}
		public override bool Equals(object obj)
		{
			WatershedForecast o = (WatershedForecast)obj;
			if (o == null)
			{
				return false;
			}
			if (this.Locations.Count != o.Locations.Count)
				return false;
			foreach (Interfaces.IEnsemble e in this.Locations)
			{
				for (int i = 0; i < o.Locations.Count; i++)
				{
					if (e.LocationName.Equals(o.Locations[i].LocationName))
					{
						if (!e.Equals(o.Locations[i]))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			var hashCode = -1316565584;
			hashCode = hashCode * -1521134295 + EqualityComparer<IList<IEnsemble>>.Default.GetHashCode(Locations);
			hashCode = hashCode * -1521134295 + WatershedName.GetHashCode();
			return hashCode;
		}
	}
}
