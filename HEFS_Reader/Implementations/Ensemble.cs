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

    public Ensemble(string name, DateTime issueDate, List<List<float>> values, DateTime[] times)
    {
      _locationName = name;
      _issuanceDate = issueDate;
      _members = new List<IEnsembleMember>();

      foreach (List<float> em in values)
      {
        _members.Add(new EnsembleMember(em.ToArray(), times));
      }
    }
    public Ensemble(string name, DateTime issueDate, float[,] values, IList<long> ticks)
    {
      _locationName = name;
      _issuanceDate = issueDate;
      _members = new List<IEnsembleMember>();
      _times = ticks.Select(t => new DateTime(t)).ToArray();

      int sz = values.GetLength(1);
      float[] buf = new float[sz];
      for (int i = 0; i < values.GetLength(0); i++)
      {
        Buffer.BlockCopy(values, i * sz * sizeof(float), buf, 0, sz * sizeof(float));

        // ToArray here so because buf is mutable with the next row (_times is static so its ok)
        _members.Add(new EnsembleMember(buf.ToArray(), _times));
      }
    }
    public DateTime IssueDate => _issuanceDate;
    public DateTime ReferenceDate => _referenceDate;
    public string LocationName => _locationName;
    public Enumerations.Timesteps Timestep => _timeStep;
    public IList<IEnsembleMember> Members => _members;

    public void AddEnsembleMember(IEnsembleMember em, int ensembleMemberIndex)
    {
      while (ensembleMemberIndex > _members.Count - 1)
      {
        _members.Add(new EnsembleMember());
      }
      _members[ensembleMemberIndex] = em;
    }

    /// <summary>
    /// Returns true if the ensemble members, have the same 
    /// float values as this instance.  Assumes the ensemble members are in a 
    /// consistent order.
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override bool Equals(object other)
    {
      IEnsemble o = other as IEnsemble;
      if (o == null) return false;
      float tolerance = 0.000001f;
      if (o.Members.Count != this.Members.Count)
      {
        Console.WriteLine("Different number of members");
        return false;
      }

      for (int memberIndex = 0; memberIndex < Members.Count; memberIndex++)
      {
        this.Members[memberIndex].ComparisonTolerance = tolerance;
        if (!this.Members[memberIndex].Equals(o.Members[memberIndex])) return false;
      }

      return true;
    }

    public override int GetHashCode()
    {
      var hashCode = 1925819071;
      hashCode = hashCode * -1521134295 + IssueDate.GetHashCode();
      hashCode = hashCode * -1521134295 + ReferenceDate.GetHashCode();
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LocationName);
      hashCode = hashCode * -1521134295 + Timestep.GetHashCode();
      hashCode = hashCode * -1521134295 + EqualityComparer<IList<IEnsembleMember>>.Default.GetHashCode(Members);
      return hashCode;
    }
  }
}
