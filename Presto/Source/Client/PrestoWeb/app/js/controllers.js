'use strict';

/* Controllers */

angular.module('myApp.controllers', [])
  .controller('appsController', appsController)
  .controller('serversController', serversController)
  .controller('logController', logController)
  .controller('appController', appController)
  .controller('installsController', installsController);

function appsController($scope, appsRepository) {
    $scope.loading = 1;
    $scope.apps = null;

    $scope.gridOptions = {
        data: 'apps',
        multiSelect: false,
        enableFiltering: true,
        selectedItems: $scope.selectedApps,
        columnDefs: [{ field: 'Name', displayName: 'Application', width: "78%", resizable: true },
                     { field: 'Version', displayName: 'Version', width: "20%" }]
    };

    $scope.refresh = function (forceRefresh) {
        // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.
        appsRepository.getApps(forceRefresh, function (dataResponse, lastRefreshTime) {
            $scope.apps = dataResponse;
            $scope.lastRefreshTime = lastRefreshTime;
            $scope.loading = 0;
        });
    };

    $scope.refresh(false);
}

function serversController($scope, serversRepository) {
    $scope.loading = 1;
    $scope.servers = null;

    $scope.gridOptions = {
        data: 'servers',
        multiSelect: false,
        enableFiltering: true,
        selectedItems: $scope.selectedServers,
        columnDefs: [{ field: 'Name', displayName: 'Server', width: "78%", resizable: true },
                     { field: 'InstallationEnvironment.Name', displayName: 'Environment', width: "20%", resizable: true }]
    };

    $scope.refresh = function (forceRefresh) {
        // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.          
        serversRepository.getServers(forceRefresh, function (dataResponse, lastRefreshTime) {
            $scope.servers = dataResponse;
            $scope.lastRefreshTime = lastRefreshTime;
            $scope.loading = 0;
        });
    };

    $scope.refresh(false);
}

function logController($scope, logRepository) {
    $scope.loading = 1;
    $scope.logMessages = null;

    $scope.gridOptions = {
        data: 'logMessages',
        multiSelect: false,
        columnDefs: [{ field: 'MessageCreatedTime', displayName: 'Time', width: "20%", resizable: true },
                     { field: 'Message', displayName: 'Message', width: "58%", resizable: true },
                     { field: 'UserName', displayName: 'User', width: "20%", resizable: true }]
    };

    $scope.refresh = function (forceRefresh) {
        // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.          
        logRepository.getLogs(forceRefresh, function (dataResponse, lastRefreshTime) {
            $scope.logMessages = dataResponse;
            $scope.lastRefreshTime = lastRefreshTime;
            $scope.loading = 0;
        });
    };

    $scope.refresh(false);
}

function appController ($scope, $http, $routeParams) {
    $scope.appId = $routeParams.appId;
    var modifiedAppId = $scope.appId.replace("/", "^^");  // Because we shouldn't send slashes in a web API call.
    $http.get('http://fs-6103.fs.local/PrestoWebApi/api/app/' + modifiedAppId)
              .then(function (result) {
                  $scope.app = result.data;
              });
}

function installsController ($scope, $http) {
    $scope.loading = 1;
    $scope.installs = null;
    $scope.selectedSummaries = [];
    $scope.selectedDetails = [];

    $scope.gridOptions = {
        data: 'installs',
        multiSelect: false,
        enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
        columnDefs: [{ field: 'ApplicationName', displayName: 'App', width: "28%", resizable: true },
                     { field: 'ServerName', displayName: 'Server', width: "20%" },
                     { field: 'InstallationStart', displayName: 'Start', width: "20%" },
                     { field: 'InstallationEnd', displayName: 'End', width: "20%" },
                     { field: 'Result', displayName: 'Result', width: "10%" }]
    };

    // Act on the row selection changing.
    $scope.gridOptions.onRegisterApi = function (gridApi) {
        $scope.gridApi = gridApi;
        gridApi.selection.on.rowSelectionChanged($scope, function (row) {
            console.log(row);  // This is a nice option. It allowed my to browse the object and discover that I wanted the entity property.
            $scope.selectedSummaries.length = 0; // Truncate/clear the array. Yes, this is how it's done.
            $scope.selectedSummaries.push(row.entity);
            $scope.selectedDetails.length = 0; // The details no longer match what is selected, so clear them.
        });
    };

    $scope.gridOptions2 = {
        data: 'selectedSummaries[0].TaskDetails',
        multiSelect: false,
        enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
        columnDefs: [{ field: 'StartTime', displayName: 'Start', width: "20%", resizable: true },
                     { field: 'EndTime', displayName: 'End', width: "20%" },
                     { field: 'Details', displayName: 'Details', width: "58%" }]
    };

    // Act on the row selection changing.
    $scope.gridOptions2.onRegisterApi = function (gridApi) {
        $scope.gridApi2 = gridApi;
        gridApi.selection.on.rowSelectionChanged($scope, function (row) {
            console.log(row);  // This is a nice option. It allowed my to browse the object and discover that I wanted the entity property.
            $scope.selectedDetails.length = 0; // Truncate/clear the array. Yes, this is how it's done.
            $scope.selectedDetails.push(row.entity);
        });
    };

    $http.get('http://fs-6103.fs.local/PrestoWebApi/api/installs/')
          .then(function (result) {
              $scope.installs = result.data;
              $scope.loading = 0;
          });      
}