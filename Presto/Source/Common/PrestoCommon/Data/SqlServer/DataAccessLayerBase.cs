using System;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
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
                //InitializeDatabase();
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

        /*******************************************************************************************
         *                                  ENTITY FRAMEWORK                                       *
         *******************************************************************************************/

        public void InitializeDatabase()
        {
            System.Data.Entity.Database.SetInitializer<PrestoContext>(new DropCreateDatabaseAlways<PrestoContext>());

            this.Database.CustomVariableGroups.Add(CreateDummyCustomVariableGroup());
            this.Database.TaskBases.Add(CreateDummyTaskDosCommand());
            this.Database.TaskBases.Add(CreateDummyTaskCopyFile());

            this.Database.SaveChanges();

            foreach (TaskBase taskBase in this.Database.TaskBases)
            {
                Debug.WriteLine(taskBase.ToString());
            }

            // Q: Remove the first custom variable in the group to see if EF removes it without me having to explicitly delete it.
            // A: The variable doesn't get deleted, however the FK pointer to the variable group gets set to NULL.
            //customVariableGroups[0].CustomVariables.RemoveAt(0);
        }

        private static CustomVariableGroup CreateDummyCustomVariableGroup()
        {
            CustomVariableGroup customVariableGroup = new CustomVariableGroup() { Id = "1", Name = "Snuh" };
            customVariableGroup.CustomVariables.Add(new CustomVariable() { Id = "1", Key = "Snuh Key 1", Value = "Snuh Value 1" });

            return customVariableGroup;
        }

        private static TaskDosCommand CreateDummyTaskDosCommand()
        {
            TaskDosCommand taskDosCommand = new TaskDosCommand("dos1", 1, 4, false, "cmd", "/c snuh");
            taskDosCommand.Id = "1";

            return taskDosCommand;
        }

        private static TaskCopyFile CreateDummyTaskCopyFile()
        {
            TaskCopyFile taskCopyFile    = new TaskCopyFile();
            taskCopyFile.Description     = "Copy file 1";
            taskCopyFile.DestinationPath = "dest path 1";
            taskCopyFile.Id              = "2";
            taskCopyFile.PrestoTaskType  = Enums.TaskType.CopyFile;
            taskCopyFile.Sequence        = 5;
            taskCopyFile.SourceFileName  = "source file 1";
            taskCopyFile.SourcePath      = "source path 1";
            taskCopyFile.TaskSucceeded   = false;

            return taskCopyFile;
        }
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

        // This isn't necessary.
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);  // ToDo: Should this be called?

        //    // Specify the many-many relationship between app and group
        //    modelBuilder.Entity<CustomVariableGroup>()
        //        .HasMany(g => g.Applications)
        //        .WithMany(a => a.CustomVariableGroups)
        //        .Map(m =>
        //            {
        //                m.MapLeftKey("CustomVariableGroup_IdForEf");
        //                m.MapRightKey("Application_IdForEf");
        //                m.ToTable("CustomVariableGroupApplications");
        //            });
        //}
    }
}
