using System;
using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Entities;
using PrestoServer.Data.Interfaces;
using Raven.Client.Linq;

namespace PrestoServer.Data.RavenDb
{
    public class CustomVariableGroupData : DataAccessLayerBase, ICustomVariableGroupData
    {
        public IEnumerable<CustomVariableGroup> GetAll()
        {
            return ExecuteQuery<IEnumerable<CustomVariableGroup>>(() =>
            {
                IEnumerable<CustomVariableGroup> customGroups =
                    QueryAndSetEtags(session =>
                        session.Query<CustomVariableGroup>()
                        .Customize(x => x.WaitForNonStaleResults())
                        .Take(int.MaxValue))
                        .AsEnumerable().Cast<CustomVariableGroup>();

                // Note: We can't have this Where() filter in the query because it's using a new property
                //       (Deleted) and the documents in the DB don't have this property yet.
                return customGroups.Where(x => x.Deleted == false);
            });
        }        

        public CustomVariableGroup GetByName(string name)
        {
            return ExecuteQuery<CustomVariableGroup>(() =>
            {
                CustomVariableGroup customVariableGroup =
                    QuerySingleResultAndSetEtag(session => session.Query<CustomVariableGroup>()
                        .Customize(x => x.WaitForNonStaleResults())
                        .Where(customGroup => customGroup.Name == name)
                        .FirstOrDefault())
                        as CustomVariableGroup;

                return customVariableGroup;
            });
        }

        public CustomVariableGroup GetById(string id)
        {
            return ExecuteQuery<CustomVariableGroup>(() =>
            {
                CustomVariableGroup customVariableGroup =
                    QuerySingleResultAndSetEtag(session => session
                        .Include<CustomVariableGroup>(customGroup => customGroup.Id == id)
                        .Load<CustomVariableGroup>(id))
                        as CustomVariableGroup;

                return customVariableGroup;
            });
        }

        public void Delete(CustomVariableGroup customVariableGroup)
        {
            if (customVariableGroup == null) { throw new ArgumentNullException("customVariableGroup"); }

            VerifyGroupNotUsedByApp(customVariableGroup);

            VerifyGroupNotUsedByServer(customVariableGroup);

            VerifyGroupNotUsedByInstallationSummary(customVariableGroup);

            // Since there is no referential integrity in RavenDB, set Deleted to true and save.
            customVariableGroup.Deleted = true;            
            new GenericData().Save(customVariableGroup);
        }

        public void Save(CustomVariableGroup customVariableGroup)
        {
            new GenericData().Save(customVariableGroup);
        }

        private static void VerifyGroupNotUsedByInstallationSummary(CustomVariableGroup customVariableGroup)
        {
            EntityBase installationSummary = QuerySingleResultAndSetEtag(session => session.Query<InstallationSummary>()
                .Where(x => x.ApplicationWithOverrideVariableGroup.CustomVariableGroupId == customVariableGroup.Id)
                .FirstOrDefault());

            if (installationSummary != null)
            {
                // Can't delete. An installation summary references this group.
                throw new InvalidOperationException("Group could not be deleted because it is included in an installation summary.");
            }
        }

        private static void VerifyGroupNotUsedByServer(CustomVariableGroup customVariableGroup)
        {
            EntityBase server = QuerySingleResultAndSetEtag(session => session.Query<ApplicationServer>()
                .Where(x => x.CustomVariableGroupIds.Any(y => y == customVariableGroup.Id) ||
                            x.CustomVariableGroupIdsForAllAppWithGroups.Any(y => y == customVariableGroup.Id))
                .FirstOrDefault());

            if (server != null)
            {
                // Can't delete. An app server, or app with group, references this group.
                throw new InvalidOperationException("Group could not be deleted because it is being used by an app server.");
            }
        }

        private static void VerifyGroupNotUsedByApp(CustomVariableGroup customVariableGroup)
        {
            EntityBase app = QuerySingleResultAndSetEtag(session => session.Query<Application>()
                .Where(x => x.CustomVariableGroupIds.Any(y => y == customVariableGroup.Id))
                .FirstOrDefault());

            if (app != null)
            {
                // Can't delete. An app references this group.
                throw new InvalidOperationException("Group could not be deleted because it is being used by an app.");
            }
        }
    }
}
