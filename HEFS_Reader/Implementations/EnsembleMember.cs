using System;
using System.Collections.Generic;
using System.Text;

namespace HEFS_Reader.Implementations
{
	class EnsembleMember : Interfaces.IEnsembleMember
	{
		private DateTime[] _times;
		private float[] _values;
		public DateTime[] getTimes()
		{
			return _times;
		}

		public float[] getValues()
		{
			return _values;
		}
	}
}
