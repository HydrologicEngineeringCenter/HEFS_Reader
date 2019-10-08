using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Returns true if the ensemble members, have the same 
        /// float values as this instance.  Assumes the ensemble members are in a 
        /// consistent order.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool HasSameData(IEnsemble e, float tolerance = 0.000001f)
        {

            if (e.Members.Count != Members.Count)
            {
                Console.WriteLine("Different number of members");
                return false;
            }

            var diff = Members.Zip(e.Members, (first, second) => first.Values.Length == second.Values.Length);
            if( diff.Any(x => x=false))
            {
                Console.WriteLine("different length of member values ");
                return false;
            }

         //   var diff2 = Members.Zip(e.Members, (first, second) =>
           //                    first.Values.Zip(second.Values, (a, b) => Math.Abs(a - b) > tolerance));

            for (int memberIndex = 0; memberIndex < Members.Count; memberIndex++)
            {
                var a = Members[memberIndex].Values;
                var b = e.Members[memberIndex].Values;
                for (int i = 0; i < a.Length; i++)
                {
                    if( Math.Abs( a[i] - b[i]) > tolerance)
                    {
                        Console.WriteLine("difference exceeds tolerance");
                        Console.WriteLine(this.LocationName+" member index = "+memberIndex+" "+this._times[i]);
                      
                        return false;
                    }
                }
            }
             
            return true;
        }

    }
}
