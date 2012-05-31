using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.SqlServer
{
    public abstract class DataAccessLayerBase
    {
        private static bool _databaseInitialized;

        protected PrestoContext Database { get; private set; }

        protected DataAccessLayerBase()
        {
            this.Database = new PrestoContext();

            if (!_databaseInitialized)
            {
                InitializeDatabase();
                _databaseInitialized = true;
            }
        }

        protected void SaveChanges<T>(T entity) where T : EntityBase
        {
            this.Database.Entry<T>(entity).State = GetState(entity);
            this.Database.SaveChanges();
        }

        protected static EntityState GetState(EntityBase entityBase)
        {
            if (entityBase == null) { throw new ArgumentNullException("entityBase"); }

            if (entityBase.IdForEf >= 1) { return EntityState.Modified; }

            return EntityState.Added;
        }

        protected static int[] GetGroupIds(List<CustomVariableGroup> groups)
        {
            if (groups == null) { throw new ArgumentNullException("groups"); }

            int[] groupIds = new int[groups.Count];

            int i = 0;
            foreach (CustomVariableGroup group in groups)
            {
                groupIds[i] = group.IdForEf;
                i++;
            }

            return groupIds;
        }

        /*******************************************************************************************
         *                                  ENTITY FRAMEWORK                                       *
         *******************************************************************************************/

        public static void InitializeDatabase()
        {
            System.Data.Entity.Database.SetInitializer<PrestoContext>(new DropCreateDatabaseAlways<PrestoContext>());

            //this.Database.CustomVariableGroups.Add(CreateDummyCustomVariableGroup());
        }

        //private static CustomVariableGroup CreateDummyCustomVariableGroup()
        //{
        //    CustomVariableGroup customVariableGroup = new CustomVariableGroup() { Id = "1", Name = "Snuh" };
        //    customVariableGroup.CustomVariables.Add(new CustomVariable() { Id = "1", Key = "Snuh Key 1", Value = "Snuh Value 1" });

        //    return customVariableGroup;
        //}
    }

    public class PrestoContext : DbContext
    {
        public DbSet<Application>         Applications          { get; set; }
        public DbSet<ApplicationServer>   ApplicationServers    { get; set; }        
        public DbSet<CustomVariable>      CustomVariables       { get; set; }
        public DbSet<CustomVariableGroup> CustomVariableGroups  { get; set; }
        public DbSet<ForceInstallation>   ForceInstallations    { get; set; }
        public DbSet<GlobalSetting>       GlobalSettings        { get; set; }
        public DbSet<InstallationSummary> InstallationSummaries { get; set; }
        public DbSet<LogMessage>          LogMessages           { get; set; }
        public DbSet<PingRequest>         PingRequests          { get; set; }
        public DbSet<PingResponse>        PingResponses         { get; set; }
        public DbSet<TaskBase>            TaskBases             { get; set; }
        
        public DbSet<ApplicationWithOverrideVariableGroup> ApplicationWithOverrideVariableGroups { get; set; }
    }
}
