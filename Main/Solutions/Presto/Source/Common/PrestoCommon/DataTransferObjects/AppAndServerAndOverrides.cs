using System;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;

namespace PrestoCommon.DataTransferObjects
{
    public class AppAndServerAndOverrides
    {
        public Application Application { get; set; }

        public ApplicationServer Server { get; set; }

        public PrestoObservableCollection<CustomVariableGroup> Overrides { get; set; }

        public DateTime EndDate { get; set; }
    }
}
