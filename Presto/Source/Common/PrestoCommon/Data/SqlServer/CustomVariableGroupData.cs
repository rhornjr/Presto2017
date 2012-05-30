using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.SqlServer
{
    public class CustomVariableGroupData : DataAccessLayerBase, ICustomVariableGroupData
    {
        public IEnumerable<CustomVariableGroup> GetAll()
        {
            return this.Database.CustomVariableGroups
                .Include(x => x.CustomVariables)
                .ToList();
        }

        public CustomVariableGroup GetByName(string name)
        {
            return this.Database.CustomVariableGroups
                .Include(x => x.CustomVariables)
                .Where(x => x.Name == name)
                .FirstOrDefault();
        }

        public CustomVariableGroup GetById(string id)
        {
            int idAsInt = Convert.ToInt32(id, CultureInfo.InvariantCulture);

            return this.Database.CustomVariableGroups
                .Include(x => x.CustomVariables)
                .Where(x => x.IdForEf == idAsInt)
                .FirstOrDefault();
        }

        public void Save(CustomVariableGroup customVariableGroup)
        {
            this.SaveChanges<CustomVariableGroup>(customVariableGroup);
        }        
    }
}
