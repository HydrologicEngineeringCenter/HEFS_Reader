using System;
using System.Collections.Generic;
using System.Text;
using HEFS_Reader.Interfaces;

namespace HEFS_Reader.Implementations
{
	class Ensemble : Interfaces.IEnsemble
	{
		private DateTime _issuanceDate;
		private DateTime _referenceDate;
		private Enumerations.Timesteps _timeStep;
		private string _locationName;
		private DateTime _times;
		private IEnsembleMember[] _members;
		public DateTime getIssueDate()
		{
			return _issuanceDate;
		}

		public DateTime getRefereceDate()
		{
			return _referenceDate;
		}
		public string getLocationName()
		{
			return _locationName;
		}

		public Enumerations.Timesteps getTimestep()
		{
			return _timeStep;
		}

		public IEnsembleMember[] getMembers()
		{
			return _members;
		}
	}
}
