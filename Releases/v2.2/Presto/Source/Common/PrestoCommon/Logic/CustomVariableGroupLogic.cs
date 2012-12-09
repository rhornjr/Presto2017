using System.Collections.Generic;
using PrestoCommon.Data;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    public static class CustomVariableGroupLogic
    {
        public static IEnumerable<CustomVariableGroup> GetAll()
        {
            return DataAccessFactory.GetDataInterface<ICustomVariableGroupData>().GetAll();
        }

        public static CustomVariableGroup GetByName(string name)
        {
            return DataAccessFactory.GetDataInterface<ICustomVariableGroupData>().GetByName(name);
        }

        public static CustomVariableGroup GetById(string id)
        {
            return DataAccessFactory.GetDataInterface<ICustomVariableGroupData>().GetById(id);
        }

        public static void Delete(CustomVariableGroup customVariableGroup)
        {
            DataAccessFactory.GetDataInterface<ICustomVariableGroupData>().Delete(customVariableGroup);
        }

        public static void Save(CustomVariableGroup customVariableGroup)
        {
            DataAccessFactory.GetDataInterface<ICustomVariableGroupData>().Save(customVariableGroup);
        }
    }
}
