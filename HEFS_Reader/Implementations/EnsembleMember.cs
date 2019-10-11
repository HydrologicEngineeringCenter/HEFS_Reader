using System;
using System.Collections.Generic;
using System.Text;
using HEFS_Reader.Interfaces;

namespace HEFS_Reader.Implementations
{
	public class EnsembleMember : Interfaces.IEnsembleMember
	{
		private IList<DateTime> _times;
		private IList<float> _values;
		private float _comparisonTolerance;
		public EnsembleMember()
		{
			_times = new List<DateTime>();
			_values = new List<float>();
		}
		public EnsembleMember(IList<float> em, IList<DateTime> times)
		{
			_values = em;
			_times = times;
		}

		public IList<DateTime> Times
		{
			get
			{
				return _times;
			}
		}

		public IList<float> Values
		{
			get
			{
				return _values;
			}
		}

		public float ComparisonTolerance { get { return _comparisonTolerance; } set { _comparisonTolerance = value; } }

		public override bool Equals(Object other)
		{
			IEnsembleMember o = other as IEnsembleMember;
			if (o != null)
			{
				if (o.Times.Count != this.Times.Count) return false;
				if (o.Values.Count != this.Values.Count) return false;
				if (o.Times[0] != this.Times[0]) return false;//assumes fixed timestep.
				for (int memberIdx = 0; memberIdx < Values.Count; memberIdx++)
				{
          if (Math.Abs(this.Values[memberIdx] - o.Values[memberIdx]) > ComparisonTolerance)
          {
            Console.WriteLine("exceeded tolerance, memberIdx = " + memberIdx);
            return false;
          }
				}
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			var hashCode = -2055526408;
			hashCode = hashCode * -1521134295 + EqualityComparer<IList<DateTime>>.Default.GetHashCode(Times);
			hashCode = hashCode * -1521134295 + EqualityComparer<IList<float>>.Default.GetHashCode(Values);
			return hashCode;
		}
	}
}
