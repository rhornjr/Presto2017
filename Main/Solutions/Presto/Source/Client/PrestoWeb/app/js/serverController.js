(function () {

    'use strict';

    angular.module('myApp.controllers').controller('serverController', serverController);

    // ------------------------------- Server Controller -------------------------------

    function serverController($scope, $rootScope, $http, $routeParams, uiGridConstants) {
        $scope.loading = 1;
        $scope.server = null;
        $scope.serverId = $routeParams.serverId;
        $scope.selectedAppsWithGroup = [];

        $scope.gridOptions = {
            data: 'server.ApplicationsWithOverrideGroup',
            multiSelect: false,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            enableFiltering: false,
            columnDefs: [{ field: 'Application.Name', displayName: 'App', width: "26%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 } },
                         { field: 'Application.Version', displayName: 'Version', width: "12%" },
                         { field: 'CustomVariableGroupNames', displayName: 'Overrides', width: "48%" },
                         { field: 'Enabled', displayName: 'Enabled', width: "12%" }]
        };

        $scope.gridOptions2 = {
            data: 'server.CustomVariableGroups',
            multiSelect: false,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            columnDefs: [{ field: 'Name', displayName: 'Name', width: "98%", resizable: true }]
        };

        $http.get('/PrestoWeb/api/server/' + $scope.serverId)
                  .then(function (result) {
                      $scope.server = result.data;
                      $scope.loading = 0;
                  },
                  function (result) {
                      $scope.loading = 0;
                      alert("An error occurred and the server could not be loaded.");
                  });

        // Act on the row selection changing.
        $scope.gridOptions.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                console.log(row);  // This is a nice option. It allowed my to browse the object and discover that I wanted the entity property.
                $scope.selectedAppsWithGroup.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.selectedAppsWithGroup.push(row.entity);
            });
        };

        $scope.install = function () {
            var entityContainer = {
                server: $scope.server,
                appWithGroup: $scope.selectedAppsWithGroup[0]
            }

            $.ajax({
                url: '/PrestoWeb/api/server/installapp',
                type: 'POST',
                data: JSON.stringify(entityContainer),
                contentType: "application/json",
                success: installRequestSucceeded($rootScope),
                error: function (jqXHR, textStatus, errorThrown) {
                    console.log("Install request failed: " + textStatus);
                }
            });
        };
    }

    function installRequestSucceeded(rootScope) {
        rootScope.setUserMessage("Install request sent successfully");
    }

})();