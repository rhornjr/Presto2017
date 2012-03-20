﻿// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Activatable", Scope = "type", Target = "PrestoCommon.Entities.ActivatableEntity")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Activatable", Scope = "type", Target = "PrestoCommon.Misc.ActivatableCollection`1")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Qa", Scope = "member", Target = "PrestoCommon.Enums.DeploymentEnvironment.#Qa")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "o", Scope = "namespace", Target = "PrestoCommon.Data.db4o")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "db", Scope = "namespace", Target = "PrestoCommon.Data.db4o")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db", Scope = "namespace", Target = "PrestoCommon.Data.RavenDb")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Etags", Scope = "member", Target = "PrestoCommon.Data.RavenDb.DataAccessLayerBase.#CacheEtags(System.Collections.Generic.IEnumerable`1<PrestoCommon.Entities.EntityBase>,Raven.Client.IDocumentSession)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Etag", Scope = "member", Target = "PrestoCommon.Data.RavenDb.DataAccessLayerBase.#RetrieveEtagFromCache(PrestoCommon.Entities.EntityBase)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Etags", Scope = "member", Target = "PrestoCommon.Data.RavenDb.DataAccessLayerBase.#QueryAndCacheEtags(System.Func`2<Raven.Client.IDocumentSession,System.Collections.Generic.IEnumerable`1<PrestoCommon.Entities.EntityBase>>)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Etag", Scope = "member", Target = "PrestoCommon.Data.RavenDb.DataAccessLayerBase.#QuerySingleResultAndCacheEtag(System.Func`2<Raven.Client.IDocumentSession,PrestoCommon.Entities.EntityBase>)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Etag", Scope = "member", Target = "PrestoCommon.Data.RavenDb.DataAccessLayerBase.#CacheEtag(PrestoCommon.Entities.EntityBase,Raven.Client.IDocumentSession)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "mostRecentInstallationSummary", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#ForceInstallationShouldHappenBasedOnTimeAndEnvironment(PrestoCommon.Entities.InstallationSummary,PrestoCommon.Entities.ApplicationWithOverrideVariableGroup)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InstallationSummary", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#InstallationSummaryExists(System.Collections.Generic.IEnumerable`1<PrestoCommon.Entities.InstallationSummary>)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InstallationStart", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#ForceInstallationShouldHappenBasedOnTimeAndEnvironment(PrestoCommon.Entities.InstallationSummary,PrestoCommon.Entities.ApplicationWithOverrideVariableGroup)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ForceInstallationTime", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#ForceInstallationExists(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ForceInstallationTime", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#ForceInstallationShouldHappenBasedOnTimeAndEnvironment(PrestoCommon.Entities.InstallationSummary,PrestoCommon.Entities.ApplicationWithOverrideVariableGroup)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ForceInstallationEnvironment", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#ForceInstallationShouldHappenBasedOnTimeAndEnvironment(PrestoCommon.Entities.InstallationSummary,PrestoCommon.Entities.ApplicationWithOverrideVariableGroup)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ForceInstallation", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#ForceInstallationExists(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ForceInstallation", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#ForceInstallationShouldHappenBasedOnTimeAndEnvironment(PrestoCommon.Entities.InstallationSummary,PrestoCommon.Entities.ApplicationWithOverrideVariableGroup)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DeploymentEnvironment", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#ForceInstallationShouldHappenBasedOnTimeAndEnvironment(PrestoCommon.Entities.InstallationSummary,PrestoCommon.Entities.ApplicationWithOverrideVariableGroup)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "appWithGroup", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#ForceInstallationShouldHappenBasedOnTimeAndEnvironment(PrestoCommon.Entities.InstallationSummary,PrestoCommon.Entities.ApplicationWithOverrideVariableGroup)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ApplicationWithOverrideVariableGroup", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#ForceInstallationExists(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ApplicationWithOverrideVariableGroup", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#AppGroupEnabled(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ApplicationWithOverrideVariableGroup", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#ForceInstallIsThisAppWithGroup(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ApplicationWithGroupToForceInstall", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#ForceInstallIsThisAppWithGroup(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ApplicationServer", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#ForceInstallIsThisAppWithGroup(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dal", Scope = "member", Target = "PrestoCommon.Data.RavenDb.DataAccessLayerBase.#SetAsInitialDalInstanceAndCreateSession()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Etags", Scope = "member", Target = "PrestoCommon.Data.RavenDb.DataAccessLayerBase.#QueryAndCacheEtags(System.Func`2<Raven.Client.IDocumentSession,System.Linq.IQueryable`1<PrestoCommon.Entities.EntityBase>>)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Etags", Scope = "member", Target = "PrestoCommon.Data.RavenDb.DataAccessLayerBase.#QueryAndDoNotCacheEtags(System.Func`2<Raven.Client.IDocumentSession,System.Linq.IQueryable`1<PrestoCommon.Entities.EntityBase>>)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Etag", Scope = "member", Target = "PrestoCommon.Data.RavenDb.DataAccessLayerBase.#QuerySingleResultAndDoNotCacheEtag(System.Func`2<Raven.Client.IDocumentSession,PrestoCommon.Entities.EntityBase>)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Etag", Scope = "member", Target = "PrestoCommon.Entities.EntityBase.#Etag")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DeploymentEnvironment", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#LogForceInstallBasedOnInstallationSummary(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup,System.DateTime,PrestoCommon.Entities.InstallationSummary,System.Boolean)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ForceInstallation", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#LogForceInstallBasedOnInstallationSummary(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup,System.DateTime,PrestoCommon.Entities.InstallationSummary,System.Boolean)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ForceInstallationEnvironment", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#LogForceInstallBasedOnInstallationSummary(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup,System.DateTime,PrestoCommon.Entities.InstallationSummary,System.Boolean)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ForceInstallationTime", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#LogForceInstallBasedOnInstallationSummary(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup,System.DateTime,PrestoCommon.Entities.InstallationSummary,System.Boolean)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InstallationStart", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#LogForceInstallBasedOnInstallationSummary(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup,System.DateTime,PrestoCommon.Entities.InstallationSummary,System.Boolean)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "appWithGroup", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#LogForceInstallBasedOnInstallationSummary(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup,System.DateTime,PrestoCommon.Entities.InstallationSummary,System.Boolean)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "mostRecentInstallationSummary", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#LogForceInstallBasedOnInstallationSummary(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup,System.DateTime,PrestoCommon.Entities.InstallationSummary,System.Boolean)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DeploymentEnvironment", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#LogForceInstallExistsWithNoInstallationSummaries(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup,System.DateTime,System.Boolean)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ForceInstallation", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#LogForceInstallExistsWithNoInstallationSummaries(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup,System.DateTime,System.Boolean)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ForceInstallationEnvironment", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#LogForceInstallExistsWithNoInstallationSummaries(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup,System.DateTime,System.Boolean)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ForceInstallationTime", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#LogForceInstallExistsWithNoInstallationSummaries(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup,System.DateTime,System.Boolean)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "appWithGroup", Scope = "member", Target = "PrestoCommon.Entities.ApplicationServer.#LogForceInstallExistsWithNoInstallationSummaries(PrestoCommon.Entities.ApplicationWithOverrideVariableGroup,System.DateTime,System.Boolean)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db", Scope = "namespace", Target = "PrestoCommon.Data.RavenDb.Indexes")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string", Scope = "member", Target = "PrestoCommon.Entities.CustomVariableGroup.#GetCustomVariableStringsWithinBiggerString(System.String)")]
