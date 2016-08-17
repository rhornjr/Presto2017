﻿(function () {

    'use strict';

    angular.module('myApp.controllers').controller('serversController', serversController);

    // ------------------------------- Servers Controller -------------------------------

    function serversController($scope, $rootScope, $routeParams, serversRepository, serversState, $window, uiGridConstants) {
        $scope.state = serversState;

        $scope.gridOptions = {
            multiSelect: false,
            enableColumnResizing: true,
            enableFiltering: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            selectedItems: $scope.state.selectedServers,
            columnDefs: [{ field: 'Name', displayName: 'Server', width: "78%", resizable: true, filter: { condition: uiGridConstants.filter.CONTAINS } },
                         { field: 'InstallationEnvironment', displayName: 'Environment', width: "20%", resizable: true }]
        };

        $scope.gridOptions.data = $scope.state.servers;

        $scope.refresh = function (forceRefresh) {
            $scope.loading = 1;
            // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.          
            serversRepository.getServers(forceRefresh, function (dataResponse) {
                $scope.state.servers = dataResponse;
                $scope.gridOptions.data = $scope.state.servers;
                $scope.loading = 0;
            });
        };

        // Act on the row selection changing.
        $scope.gridOptions.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                console.log(row);  // This is a nice option. It allowed me to browse the object and discover that I wanted the entity property.
                $scope.state.selectedServers.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.state.selectedServers.push(row.entity);
            });
        };

        $scope.addServer = function () {
            $window.location.href = '/PrestoWeb/app/#/server';
        };

        $scope.editServer = function () {
            var modifiedServerId = $scope.state.selectedServers[0].Id.replace("/", "^^");  // Because we shouldn't send slashes in a URL.
            $window.location.href = '/PrestoWeb/app/#/server/' + modifiedServerId;
        };

        // If the servers haven't been loaded yet, or we've come here via a link telling us to load the list, then load the list.
        if (!$scope.state.servers || $routeParams.showList == 1) {
            $scope.refresh(false);
        }
        else {
            // If a server has been selected, go back to it.
            // Note: This is in a timeout because we can't redirect in the same turn as loading this page.
            //       The timeout callback happens in a different turn, so it works. If we called $scope.editServer()
            //       directly (not in the timeout), we end up in an infinite loop calling this line.
            if ($scope.state.selectedServers[0]) {
                setTimeout(function () {
                    $scope.editServer();
                }, 100);
            }
        }
    }

})();