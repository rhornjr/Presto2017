using System.Collections.Generic;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.Interfaces
{
    public interface ICustomVariableGroupData
    {
        IEnumerable<CustomVariableGroup> GetAll();

        CustomVariableGroup GetByName(string name);

        CustomVariableGroup GetById(string id);

        void Delete(CustomVariableGroup customVariableGroup);

        void Save(CustomVariableGroup customVariableGroup);
    }
}
