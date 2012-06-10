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

        public void Save(CustomVariableGroup newGroup)
        {
            if (newGroup == null) { throw new ArgumentNullException("newGroup"); }

            CustomVariableGroup groupFromContext;

            if (newGroup.IdForEf == 0)  // New group
            {
                groupFromContext = new CustomVariableGroup();
                this.Database.CustomVariableGroups.Add(groupFromContext);
                this.Database.Entry(groupFromContext).CurrentValues.SetValues(newGroup);  // Set all scalar properties.                
            }
            else
            {
                groupFromContext = this.Database.CustomVariableGroups
                    .Include(x => x.CustomVariables)
                    .Single(x => x.IdForEf == newGroup.IdForEf);
            }

            AddVariablesToGroup(newGroup, groupFromContext);
            this.Database.SaveChanges();
        }

        private void AddVariablesToGroup(CustomVariableGroup groupNotAssociatedWithContext, CustomVariableGroup groupFromContext)
        {
            foreach (CustomVariable variabaleInNewGroup in groupNotAssociatedWithContext.CustomVariables)
            {
                if (variabaleInNewGroup.IdForEf == 0)
                {
                    groupFromContext.CustomVariables.Add(variabaleInNewGroup);
                }
                else
                {
                    CustomVariable variable = groupFromContext.CustomVariables.Single(x => x.IdForEf == variabaleInNewGroup.IdForEf);  // Get original task
                    this.Database.Entry(variable).CurrentValues.SetValues(variabaleInNewGroup);  // Update with new
                }
            }

            // Delete tasks that no longer exist within the app.
            List<CustomVariable> variablesToDelete = new List<CustomVariable>();
            foreach (CustomVariable originalVariable in groupFromContext.CustomVariables)
            {
                CustomVariable variable = groupNotAssociatedWithContext.CustomVariables.Where(x => x.IdForEf == originalVariable.IdForEf).FirstOrDefault();

                if (variable == null)
                {
                    variablesToDelete.Add(originalVariable);
                }
            }

            foreach (CustomVariable variableToDelete in variablesToDelete)
            {
                groupFromContext.CustomVariables.Remove(variableToDelete);
                this.Database.CustomVariables.Remove(variableToDelete);
            }
        }
    }
}
