using System.Collections.Generic;
using PrestoCommon.Entities;
using PrestoServer.Data;
using PrestoServer.Data.Interfaces;
using Raven.Abstractions.Exceptions;

namespace PrestoServer.Logic
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
            try
            {
                DataAccessFactory.GetDataInterface<ICustomVariableGroupData>().Save(customVariableGroup);
            }
            catch (ConcurrencyException ex)
            {
                LogicBase.SetConcurrencyUserSafeMessage(ex, customVariableGroup.Name);
                throw;
            }
        }
    }
}
