using System.Collections.Generic;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoServer.Logic;

namespace PrestoWcfService.WcfServices
{
    /// <summary>
    /// Presto service class that implements every Presto service interface. We do this so the client
    /// calls can specify specific service interfaces (IApplicationService, IServerService, etc...)
    /// and deal only with the methods for the entity they care about.
    /// </summary>
    public class PrestoService : IBaseService, IApplicationService, ICustomVariableGroupService, IServerService,
        IInstallationEnvironmentService, IInstallationSummaryService, IPingService
    {
        #region [Base Methods]

        public string Echo(string message)
        {
            return message;
        }

        public void Delete(EntityBase objectToDelete)
        {
            LogicBase.Delete(objectToDelete);
        }

        public IEnumerable<LogMessage> GetMostRecentLogMessagesByCreatedTime(int numberToRetrieve)
        {
            return LogMessageLogic.GetMostRecentByCreatedTime(numberToRetrieve);
        }

        public void SaveLogMessage(string message)
        {
            LogMessageLogic.SaveLogMessage(message);
        }

        public GlobalSetting GetGlobalSettingItem()
        {
            return GlobalSettingLogic.GetItem();
        }

        public GlobalSetting SaveGlobalSetting(GlobalSetting globalSetting)
        {
            GlobalSettingLogic.Save(globalSetting);
            return globalSetting;
        }

        #endregion

        #region [Application]

        public Application GetById(string id)
        {
            return ApplicationLogic.GetById(id);
        }

        public IEnumerable<Application> GetAllApplications()
        {
            return ApplicationLogic.GetAll();
        }

        public Application GetByName(string name)
        {
            return ApplicationLogic.GetByName(name);
        }

        public Application SaveApplication(Application application)
        {
            ApplicationLogic.Save(application);
            return application;
        }

        #endregion

        #region [Server]

        public ApplicationServer GetServerById(string id)
        {
            return ApplicationServerLogic.GetById(id);
        }

        public IEnumerable<ApplicationServer> GetAllServers()
        {
            return ApplicationServerLogic.GetAll();
        }

        public void InstallPrestoSelfUpdater(ApplicationServer appServer)
        {
            ApplicationServerLogic.InstallPrestoSelfUpdater(appServer);
        }

        public ApplicationServer SaveServer(ApplicationServer applicationServer)
        {
            ApplicationServerLogic.Save(applicationServer);
            return applicationServer;
        }

        public void SaveForceInstallations(List<ServerForceInstallation> serverForceInstallations)
        {
            ApplicationServerLogic.SaveForceInstallations(serverForceInstallations);
        }

        public void RemoveForceInstallation(ServerForceInstallation forceInstallation)
        {
            ApplicationServerLogic.RemoveForceInstallation(forceInstallation);
        }

        #endregion

        #region [InstallationEnvironment]

        public IEnumerable<InstallationEnvironment> GetAllInstallationEnvironments()
        {
            return InstallationEnvironmentLogic.GetAll();
        }

        public void Save(InstallationEnvironment environment)
        {
            InstallationEnvironmentLogic.Save(environment);
        }

        #endregion

        #region [InstallationSummary]

        public IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve)
        {
            return InstallationSummaryLogic.GetMostRecentByStartTime(numberToRetrieve);
        }

        #endregion

        #region [CustomVariableGroup]

        CustomVariableGroup ICustomVariableGroupService.GetById(string id)
        {
            return CustomVariableGroupLogic.GetById(id);
        }

        public IEnumerable<CustomVariableGroup> GetAllGroups()
        {
            return CustomVariableGroupLogic.GetAll();
        }

        public CustomVariableGroup GetCustomVariableGroupByName(string name)
        {
            return CustomVariableGroupLogic.GetByName(name);
        }

        public CustomVariableGroup SaveGroup(CustomVariableGroup customVariableGroup)
        {
            CustomVariableGroupLogic.Save(customVariableGroup);
            return customVariableGroup;
        }

        public void DeleteGroup(CustomVariableGroup customVariableGroup)
        {
            CustomVariableGroupLogic.Delete(customVariableGroup);
        }
        
        #endregion

        #region [Ping]

        public PingRequest GetMostRecentPingRequest()
        {
            return PingRequestLogic.GetMostRecent();
        }

        public void SavePingRequest(PingRequest pingRequest)
        {
            PingRequestLogic.Save(pingRequest);
        }

        public IEnumerable<PingResponse> GetAllForPingRequest(PingRequest pingRequest)
        {
            return PingResponseLogic.GetAllForPingRequest(pingRequest);
        }

        #endregion
    }
}
