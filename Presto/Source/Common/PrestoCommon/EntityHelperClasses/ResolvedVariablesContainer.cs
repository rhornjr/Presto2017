using System.Collections.Generic;
using PrestoCommon.Entities;

namespace PrestoCommon.EntityHelperClasses
{
    public class ResolvedVariablesContainer
    {
        public IEnumerable<CustomVariable> Variables { get; set; }

        public int NumberOfProblems { get; set; }

        public string SupplementalStatusMessage { get; set; }
    }
}
