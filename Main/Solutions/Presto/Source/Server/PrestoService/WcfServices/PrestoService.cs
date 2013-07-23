using System;
using System.Collections.Generic;
using System.ServiceModel;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Misc;
using PrestoServer.Logic;
using Xanico.Core.Wcf;

namespace PrestoWcfService.WcfServices
{
    /// <summary>
    /// Presto service class that implements every Presto service interface. We do this so the client
    /// calls can specify specific service interfaces (IApplicationService, IServerService, etc...)
    /// and deal only with the methods for the entity they care about.
    /// </summary>
    [ServiceBehavior(MaxItemsInObjectGraph = int.MaxValue)]
    public class PrestoService : IBaseService, IApplicationService, ICustomVariableGroupService, IServerService,
        IInstallationEnvironmentService, IInstallationSummaryService, IPingService, ISecurityService
    {
        #region [Base Methods]

        public string Echo(string message)
        {
            return message;
        }

        public void Delete(EntityBase objectToDelete)
        {
            Invoke(() => LogicBase.Delete(objectToDelete));
        }

        public IEnumerable<LogMessage> GetMostRecentLogMessagesByCreatedTime(int numberToRetrieve)
        {
            return Invoke(() => LogMessageLogic.GetMostRecentByCreatedTime(numberToRetrieve));
        }

        public void SaveLogMessage(string message)
        {
            Invoke(() => LogMessageLogic.SaveLogMessage(message));
        }

        public GlobalSetting GetGlobalSettingItem()
        {
            return Invoke(() => GlobalSettingLogic.GetItem());
        }

        public GlobalSetting SaveGlobalSetting(GlobalSetting globalSetting)
        {
            return Invoke(() =>
            {
                GlobalSettingLogic.Save(globalSetting);
                return globalSetting;
            });
        }

        #endregion

        #region [Application]

        public Application GetById(string id)
        {
            return Invoke(() => ApplicationLogic.GetById(id));
        }

        public IEnumerable<Application> GetAllApplications()
        {
            return Invoke(() => ApplicationLogic.GetAll());
        }

        public Application GetByName(string name)
        {
            return Invoke(() => ApplicationLogic.GetByName(name));
        }

        public Application SaveApplication(Application application)
        {
            return Invoke(() =>
            {
                ApplicationLogic.Save(application);
                return application;
            });
        }

        #endregion

        #region [Server]

        public ApplicationServer GetServerById(string id)
        {
            return Invoke(() => ApplicationServerLogic.GetById(id));
        }

        public IEnumerable<ApplicationServer> GetAllServers()
        {
            return Invoke(() => ApplicationServerLogic.GetAll());
        }

        public IEnumerable<ApplicationServer> GetAllServersSlim()
        {
            return Invoke(() => ApplicationServerLogic.GetAllSlim());
        }

        public void InstallPrestoSelfUpdater(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            Invoke(() => ApplicationServerLogic.InstallPrestoSelfUpdater(appServer, appWithGroup));
        }

        public ApplicationServer SaveServer(ApplicationServer applicationServer)
        {
            return Invoke(() =>
            {
                ApplicationServerLogic.Save(applicationServer);
                return applicationServer;
            });
        }

        public void SaveForceInstallations(List<ServerForceInstallation> serverForceInstallations)
        {
            Invoke(() => ApplicationServerLogic.SaveForceInstallations(serverForceInstallations));
        }

        public void RemoveForceInstallation(ServerForceInstallation forceInstallation)
        {
            Invoke(() => ApplicationServerLogic.RemoveForceInstallation(forceInstallation));
        }

        #endregion

        #region [InstallationEnvironment]

        public IEnumerable<InstallationEnvironment> GetAllInstallationEnvironments()
        {
            return Invoke(() => InstallationEnvironmentLogic.GetAll());
        }

        public void Save(InstallationEnvironment environment)
        {
            Invoke(() => InstallationEnvironmentLogic.Save(environment));
        }

        #endregion

        #region [InstallationSummary]

        public IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve)
        {
            return Invoke(() => InstallationSummaryLogic.GetMostRecentByStartTime(numberToRetrieve));
        }

        #endregion

        #region [CustomVariableGroup]

        CustomVariableGroup ICustomVariableGroupService.GetById(string id)
        {
            return Invoke(() => CustomVariableGroupLogic.GetById(id));
        }

        public IEnumerable<CustomVariableGroup> GetAllGroups()
        {
            return Invoke(() => CustomVariableGroupLogic.GetAll());
        }

        public CustomVariableGroup GetCustomVariableGroupByName(string name)
        {
            return Invoke(() => CustomVariableGroupLogic.GetByName(name));
        }

        public CustomVariableGroup SaveGroup(CustomVariableGroup customVariableGroup)
        {
            return Invoke(() =>
            {
                CustomVariableGroupLogic.Save(customVariableGroup);
                return customVariableGroup;
            });
        }

        public void DeleteGroup(CustomVariableGroup customVariableGroup)
        {
            Invoke(() => CustomVariableGroupLogic.Delete(customVariableGroup));
        }
        
        #endregion

        #region [Ping]

        public PingRequest GetMostRecentPingRequest()
        {
            return Invoke(() => PingRequestLogic.GetMostRecent());
        }

        public PingRequest SavePingRequest(PingRequest pingRequest)
        {
            return Invoke(() =>
            {
                PingRequestLogic.Save(pingRequest);
                return pingRequest;
            });
        }

        public IEnumerable<PingResponse> GetAllForPingRequest(PingRequest pingRequest)
        {
            return Invoke(() => PingResponseLogic.GetAllForPingRequest(pingRequest));
        }

        #endregion

        #region [Security]

        public IEnumerable<AdGroupWithRoles> GetAllAdGroupWithRoles()
        {
            return Invoke(() => SecurityLogic.GetAll());
        }

        public AdGroupWithRoles SaveAdGroupWithRoles(AdGroupWithRoles adGroupWithRoles)
        {
            Invoke(() => SecurityLogic.Save(adGroupWithRoles));
            return adGroupWithRoles;
        }

        public ActiveDirectoryInfo GetActiveDirectoryInfo()
        {
            return Invoke(() => SecurityLogic.GetActiveDirectoryInfo());
        }

        public ActiveDirectoryInfo SaveActiveDirectoryInfo(ActiveDirectoryInfo adInfo)
        {
            Invoke(() => SecurityLogic.SaveActiveDirectoryInfo(adInfo));
            return adInfo;
        }

        #endregion

        #region [Private Helper Methods]

        private static void Invoke(Action action)
        {
            // Most of the methods in this class call the Invoke() that takes a func. Some of the methods
            // don't want to use a func because they don't return anything. So, in order for all of the
            // calls to be the same, accept an action is this method, and call the other Invoke().
            // Note, we return a dummy value of 0 here just so we can call the func Invoke().

            Invoke(() => { action(); return 0; });
        }

        private static T Invoke<T>(Func<T> func)
        {
            // This method exists simply to encapsulate the exception handling for the methods in this class.
            try
            {
                return func.Invoke();
            }
            catch (Exception ex)
            {
                throw LogAndThrowFaultException(ex);
            }
        }

        private static FaultException LogAndThrowFaultException(Exception exception)
        {
            CommonUtility.ProcessException(exception);

            return new FaultException(FaultUtility.GetFaultMessage(exception));
        }

        #endregion
    }
}
