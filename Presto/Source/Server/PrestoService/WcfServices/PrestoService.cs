using System;
using System.Collections.Generic;
using System.ServiceModel;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoServer.Logic;
using Xanico.Core;
using Xanico.Core.Wcf;

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
            try
            {
                LogicBase.Delete(objectToDelete);
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public IEnumerable<LogMessage> GetMostRecentLogMessagesByCreatedTime(int numberToRetrieve)
        {
            try
            {
                return LogMessageLogic.GetMostRecentByCreatedTime(numberToRetrieve);
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public void SaveLogMessage(string message)
        {
            try
            {
                LogMessageLogic.SaveLogMessage(message);
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public GlobalSetting GetGlobalSettingItem()
        {
            try
            {
                return GlobalSettingLogic.GetItem();
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public GlobalSetting SaveGlobalSetting(GlobalSetting globalSetting)
        {
            try
            {
                GlobalSettingLogic.Save(globalSetting);
                return globalSetting;
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        #endregion

        #region [Application]

        public Application GetById(string id)
        {
            try
            {
                return ApplicationLogic.GetById(id);
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public IEnumerable<Application> GetAllApplications()
        {
            try
            {
                return ApplicationLogic.GetAll();
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public Application GetByName(string name)
        {
            try
            {
                return ApplicationLogic.GetByName(name);
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public Application SaveApplication(Application application)
        {
            try
            {
                ApplicationLogic.Save(application);
                return application;
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        #endregion

        #region [Server]

        public ApplicationServer GetServerById(string id)
        {
            try
            {
                return ApplicationServerLogic.GetById(id);
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public IEnumerable<ApplicationServer> GetAllServers()
        {
            try
            {
                return ApplicationServerLogic.GetAll();
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public void InstallPrestoSelfUpdater(ApplicationServer appServer)
        {
            try
            {
                ApplicationServerLogic.InstallPrestoSelfUpdater(appServer);
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public ApplicationServer SaveServer(ApplicationServer applicationServer)
        {
            try
            {
                ApplicationServerLogic.Save(applicationServer);
                return applicationServer;
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public void SaveForceInstallations(List<ServerForceInstallation> serverForceInstallations)
        {
            try
            {
                ApplicationServerLogic.SaveForceInstallations(serverForceInstallations);
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public void RemoveForceInstallation(ServerForceInstallation forceInstallation)
        {
            try
            {
                ApplicationServerLogic.RemoveForceInstallation(forceInstallation);
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        #endregion

        #region [InstallationEnvironment]

        public IEnumerable<InstallationEnvironment> GetAllInstallationEnvironments()
        {
            try
            {
                return InstallationEnvironmentLogic.GetAll();
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public void Save(InstallationEnvironment environment)
        {
            try
            {
                InstallationEnvironmentLogic.Save(environment);
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        #endregion

        #region [InstallationSummary]

        public IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve)
        {
            try
            {
                return InstallationSummaryLogic.GetMostRecentByStartTime(numberToRetrieve);
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        #endregion

        #region [CustomVariableGroup]

        CustomVariableGroup ICustomVariableGroupService.GetById(string id)
        {
            try
            {
                return CustomVariableGroupLogic.GetById(id);
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public IEnumerable<CustomVariableGroup> GetAllGroups()
        {
            try
            {
                return CustomVariableGroupLogic.GetAll();
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public CustomVariableGroup GetCustomVariableGroupByName(string name)
        {
            try
            {
                return CustomVariableGroupLogic.GetByName(name);
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public CustomVariableGroup SaveGroup(CustomVariableGroup customVariableGroup)
        {
            try
            {
                CustomVariableGroupLogic.Save(customVariableGroup);
                return customVariableGroup;
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public void DeleteGroup(CustomVariableGroup customVariableGroup)
        {
            try
            {
                CustomVariableGroupLogic.Delete(customVariableGroup);
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }
        
        #endregion

        #region [Ping]

        public PingRequest GetMostRecentPingRequest()
        {
            try
            {
                return PingRequestLogic.GetMostRecent();
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public void SavePingRequest(PingRequest pingRequest)
        {
            try
            {
                PingRequestLogic.Save(pingRequest);
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        public IEnumerable<PingResponse> GetAllForPingRequest(PingRequest pingRequest)
        {
            try
            {
                return PingResponseLogic.GetAllForPingRequest(pingRequest);
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        #endregion

        private static FaultException LogAndThrowFaultException(Exception exception)
        {
            Logger.LogException(exception);

            return new FaultException(FaultUtility.GetFaultMessage(exception));
        }
    }
}
