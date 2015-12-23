(function () {

    'use strict';

    angular.module('myApp.controllers').controller('serversController', serversController);

    // ------------------------------- Servers Controller -------------------------------

    function serversController($scope, $rootScope, serversRepository, $window, uiGridConstants) {
        $scope.loading = 1;
        $scope.servers = null;
        $scope.selectedServers = [];

        $scope.gridOptions = {
            data: 'servers',
            multiSelect: false,
            enableFiltering: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            selectedItems: $scope.selectedServers,
            columnDefs: [{ field: 'Name', displayName: 'Server', width: "78%", resizable: true, filter: { condition: uiGridConstants.filter.CONTAINS } },
                         { field: 'InstallationEnvironment', displayName: 'Environment', width: "20%", resizable: true }]
        };

        $scope.refresh = function (forceRefresh) {
            // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.          
            serversRepository.getServers(forceRefresh, function (dataResponse) {
                $scope.servers = dataResponse;
                $scope.loading = 0;
            });
        };

        // Act on the row selection changing.
        $scope.gridOptions.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                console.log(row);  // This is a nice option. It allowed me to browse the object and discover that I wanted the entity property.
                $scope.selectedServers.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.selectedServers.push(row.entity);
            });
        };

        $scope.editServer = function () {
            var modifiedServerId = $scope.selectedServers[0].Id.replace("/", "^^");  // Because we shouldn't send slashes in a URL.
            $window.location.href = '/PrestoWeb/app/#/server/' + modifiedServerId;
        };

        $scope.refresh(false);
    }

})();