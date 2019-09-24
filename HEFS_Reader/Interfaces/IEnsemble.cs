using System;
using System.Collections.Generic;

namespace HEFS_Reader.Interfaces
{
	public interface IEnsemble
	{
		DateTime getRefereceDate();
		DateTime getIssueDate();
		string getLocationName();
		Enumerations.Timesteps getTimestep();
		IList<IEnsembleMember> getMembers();
	}
}
