using System;
using System.Collections.Generic;
using System.Text;

namespace HEFS_Reader.Implementations
{
	public class EnsembleMember : Interfaces.IEnsembleMember
	{
		private DateTime[] _times;
		private float[] _values;

		public EnsembleMember(float[] em, DateTime[] times)
		{
			_values = em;
			_times = times;
		}

		public DateTime[] Times
		{
			get
			{
				return _times;
			}
		}

		public float[] Values
		{
			get
			{
				return _values;
			}
		}
	}
}
