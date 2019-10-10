using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
      WatershedForecast o = (WatershedForecast) obj;
      if( o == null)
      {
        return false;
      }
      if (this.Locations.Count != o.Locations.Count)
        return false;
      for (int i = 0; i < this.Locations.Count; i++)
      {
        if (!this.Locations[i].Equals(o.Locations[i]))
          return false;
      }
      return true;
    }

    
  }
}
