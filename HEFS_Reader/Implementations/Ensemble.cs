using System;
using System.Collections.Generic;
using System.Text;
using HEFS_Reader.Interfaces;

namespace HEFS_Reader.Implementations
{
	public class Ensemble : IEnsemble
	{
		private DateTime _issuanceDate;
		private DateTime _referenceDate;
		private Enumerations.Timesteps _timeStep;
		private string _locationName;
		private DateTime[] _times;//same times for all members.
		private IList<IEnsembleMember> _members;
		public Ensemble(string name, DateTime issueDate, List<List<float>> values, List<DateTime> times)
		{
			_locationName = name;
			_issuanceDate = issueDate;
			_members = new List<IEnsembleMember>();
			_times = times.ToArray();
			foreach(List<float> em in values) {
				_members.Add(new EnsembleMember(em.ToArray(), _times));
			}
			

		}
		public DateTime IssueDate
		{
			get
			{
				return _issuanceDate;
			}
		}

		public DateTime RefereceDate
		{
			get
			{
				return _referenceDate;
			}
		}
		public string LocationName
		{
			get
			{
				return _locationName;
			}
		}

		public Enumerations.Timesteps Timestep
		{
			get
			{
				return _timeStep;
			}
		}

		public IList<IEnsembleMember> Members
		{
			get
			{
				return _members;
			}
		}
	}
}
