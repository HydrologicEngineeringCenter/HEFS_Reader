using System;
using System.Collections.Generic;
using System.Text;

namespace HEFS_Reader.Interfaces
{
	public interface IEnsembleMember
	{
		DateTime[] Times { get; }
		float[] Values { get; }
	}
}
