using System;

namespace HEFS_Reader.Interfaces
{
	public interface IEnsemble
	{
		DateTime getRefereceDate();
		DateTime getIssueDate();
		Enumerations.Timesteps getTimestep();
		IEnsembleMember[] getMembers();
	}
}
