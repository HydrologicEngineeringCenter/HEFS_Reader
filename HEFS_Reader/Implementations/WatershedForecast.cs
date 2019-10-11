using HEFS_Reader.Enumerations;
using HEFS_Reader.Interfaces;
using System;
using System.Collections.Generic;

namespace HEFS_Reader.Implementations
{
	public class WatershedForecast : Interfaces.IWatershedForecast
	{
		private IList<Interfaces.IEnsemble> _ensembles;
		private Enumerations.Watersheds _watershedName;
		private DateTime _issueDate;
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

		public DateTime IssueDate
		{
			get
			{
				return _issueDate;
			}
		}

		//public WatershedForecast(IList<Interfaces.IEnsemble> ensembles, Enumerations.Watersheds watershedName)
		//{
		//	_ensembles = ensembles;
		//	_watershedName = watershedName;
		//}
		public WatershedForecast(IList<Interfaces.IEnsemble> ensembles, Enumerations.Watersheds watershedName,DateTime issueDate)
		{
			_ensembles = ensembles;
			_watershedName = watershedName;
			_issueDate = issueDate;
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
		void IWatershedForecast.AddEnsembleMember(IEnsembleMember em, int ensembleMemberIndex, string location)
		{
				if (_ensembles.Count == 0) {
					_ensembles.Add(new Ensemble(location, _issueDate, new List<List<float>>(), em.Times));
				}
			foreach (IEnsemble e in _ensembles)
			{
				if (e.LocationName.Equals(location))
				{
					e.AddEnsembleMember(em, ensembleMemberIndex);
				}
			}
				//throw new NotImplementedException();
		}

		public int CompareTo(IWatershedForecast other)
		{
			return this.IssueDate.CompareTo(other.IssueDate);
		}
	}
}
