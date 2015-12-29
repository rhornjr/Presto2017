(function () {

    'use strict';

    angular.module('myApp.controllers').controller('resolveController', resolveController);

    // ------------------------------- Modal Controllers -------------------------------

    // APP PICKER

    angular.module('myApp.controllers').controller('appPickerModalController', function ($scope, $uibModalInstance, uiGridConstants, appsRepository) {
        $scope.loading = 1;
        $scope.apps = null;
        $scope.selectedApps = [];

        $scope.gridApps = {
            data: 'apps',
            multiSelect: false,
            enableFiltering: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            selectedItems: $scope.selectedApps,
            columnDefs: [{ field: 'Name', displayName: 'Application', width: "78%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 }, filter: { condition: uiGridConstants.filter.CONTAINS } },
                         { field: 'Version', displayName: 'Version', width: "20%", sort: { direction: uiGridConstants.ASC, priority: 2 } }]
        };

        $scope.refresh = function (forceRefresh) {
            $scope.loading = 1;
            // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.
            appsRepository.getApps(forceRefresh, function (dataResponse) {
                $scope.apps = dataResponse;
                $scope.loading = 0;
            });
        };

        // Act on the row selection changing.
        $scope.gridApps.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                console.log(row.entity);  // This is a nice option. It allowed me to browse the object and discover that I wanted the entity property.
                $scope.selectedApps.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.selectedApps.push(row.entity);
            });
        };

        $scope.ok = function () {
            $uibModalInstance.close($scope.selectedApps[0]);
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss();
        };

        $scope.refresh(true);
    });

    // SERVER PICKER

    angular.module('myApp.controllers').controller('serverPickerModalController', function ($scope, $uibModalInstance, uiGridConstants, serversRepository) {
        $scope.loading = 1;
        $scope.servers = null;
        $scope.selectedServers = [];

        $scope.gridServers = {
            data: 'servers',
            multiSelect: false,
            enableFiltering: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            selectedItems: $scope.selectedApps,
            columnDefs: [{ field: 'Name', displayName: 'Server', width: "68%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 }, filter: { condition: uiGridConstants.filter.CONTAINS } },
                         { field: 'InstallationEnvironment', displayName: 'Environment', width: "30%", sort: { direction: uiGridConstants.ASC, priority: 2 } }]
        };

        $scope.refresh = function (forceRefresh) {
            $scope.loading = 1;
            // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.
            serversRepository.getServers(forceRefresh, function (dataResponse) {
                $scope.servers = dataResponse;
                $scope.loading = 0;
            });
        };

        // Act on the row selection changing.
        $scope.gridServers.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                console.log(row.entity);  // This is a nice option. It allowed me to browse the object and discover that I wanted the entity property.
                $scope.selectedServers.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.selectedServers.push(row.entity);
            });
        };

        $scope.ok = function () {
            $uibModalInstance.close($scope.selectedServers[0]);
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss();
        };

        $scope.refresh(true);
    });

    // ------------------------------- Resolve Controller -------------------------------

    function resolveController($scope, $rootScope, $http, $uibModal, uiGridConstants) {
        $scope.selectedApp;
        $scope.selectedServer;
        $scope.resolvedVariables;

        $scope.gridResolve = {
            data: 'resolvedVariables',
            multiSelect: false,
            enableFiltering: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            selectedItems: $scope.selectedApps,
            columnDefs: [{ field: 'Key', displayName: 'Variable', width: "30%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 }, filter: { condition: uiGridConstants.filter.CONTAINS } },
                         { field: 'Value', displayName: 'Value', width: "68%", sort: { direction: uiGridConstants.ASC, priority: 2 } }]
        };

        $scope.pickApp = function () {
            var modalInstance = $uibModal.open({
                templateUrl: 'partials/appPicker.html',
                controller: 'appPickerModalController',
                size: 'sm',
                //windowClass: 'modalConfirmationPosition'
                windowClass: 'app-modal-window'
            });

            modalInstance.result.then(function (app) {
                console.log("App picked", app);
                $scope.selectedApp = app;
            }, function () {
                // modal dismissed
            });
        }

        $scope.pickServer = function () {
            var modalInstance = $uibModal.open({
                templateUrl: 'partials/serverPicker.html',
                controller: 'serverPickerModalController',
                size: 'sm',
                //windowClass: 'modalConfirmationPosition'
                windowClass: 'app-modal-window'
            });

            modalInstance.result.then(function (server) {
                console.log("Server picked", server);
                $scope.selectedServer = server;
            }, function () {
                // modal dismissed
            });
        }

        $scope.resolve = function () {
            // Create *one* object because we can't pass two parameters to a web API method.
            var appAndServer = {
                application: $scope.selectedApp,
                server: $scope.selectedServer
            }

            var config = {
                url: '/PrestoWeb/api/resolve/resolveVariables',
                method: 'POST',
                data: appAndServer
            };

            $http(config)
                .then(function (response) {
                    $scope.resolvedVariables = response.data;
                    $rootScope.setUserMessage("Variables resolved");
                }, function (response) {
                    alert(response);
                });
        }
    }

})();