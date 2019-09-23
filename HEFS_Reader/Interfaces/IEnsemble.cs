using System;

namespace HEFS_Reader.Interfaces
{
	public interface IEnsemble
	{
		DateTime getRefereceDate();
		DateTime getIssueDate();
		string getLocationName();
		Enumerations.Timesteps getTimestep();
		IEnsembleMember[] getMembers();
	}
}
