using System;
using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.SqlServer
{
    public class CustomVariableGroupData : DataAccessLayerBase, ICustomVariableGroupData
    {
        public IEnumerable<CustomVariableGroup> GetAll()
        {
            return this.Database.CustomVariableGroups.ToList();
        }

        public CustomVariableGroup GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public CustomVariableGroup GetById(string id)
        {
            throw new NotImplementedException();
        }

        public void Save(CustomVariableGroup customVariableGroup)
        {
            this.SaveChanges<CustomVariableGroup>(customVariableGroup);
        }        
    }
}
