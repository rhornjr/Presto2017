(function () {

    'use strict';

    angular.module('myApp.controllers').controller('serverController', serverController);

    // ------------------------------- Server Controller -------------------------------

    function serverController($scope, $rootScope, $http, $routeParams, uiGridConstants, $uibModal, showConfirmationModal) {
        $scope.loading = 1;
        $scope.server = null;
        $scope.serverId = $routeParams.serverId;
        $scope.selectedAppsWithGroup = [];
        $scope.selectedGroups = [];

        // ---------------------------------------------------------------------------------------------------

        $scope.gridAppsWithGroup = {
            data: 'server.ApplicationsWithOverrideGroup',
            multiSelect: false,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            enableFiltering: false,
            columnDefs: [{ field: 'Application.Name', displayName: 'App', width: "26%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 } },
                         { field: 'Application.Version', displayName: 'Version', width: "12%" },
                         { field: 'CustomVariableGroupNames', displayName: 'Overrides', width: "48%" },
                         { field: 'Enabled', displayName: 'Enabled', width: "12%" }]
        };

        // ---------------------------------------------------------------------------------------------------

        $scope.gridGroups = {
            data: 'server.CustomVariableGroups',
            multiSelect: false,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            columnDefs: [{ field: 'Name', displayName: 'Name', width: "98%", resizable: true }]
        };

        // ---------------------------------------------------------------------------------------------------

        $http.get('/PrestoWeb/api/server/' + $scope.serverId)
                  .then(function (result) {
                      $scope.server = result.data;
                      $scope.loading = 0;
                  },
                  function (result) {
                      $scope.loading = 0;
                      showInfoModal.show(response.statusText, response.data);
                  });

        // ---------------------------------------------------------------------------------------------------

        // Act on the row selection changing.
        $scope.gridAppsWithGroup.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                console.log(row);  // This is a nice option. It allowed my to browse the object and discover that I wanted the entity property.
                $scope.selectedAppsWithGroup.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.selectedAppsWithGroup.push(row.entity);
            });
        };

        // ---------------------------------------------------------------------------------------------------

        // Act on the row selection changing.
        $scope.gridGroups.onRegisterApi = function (gridGroupsApi) {
            $scope.gridGroupsApi = gridGroupsApi;
            gridGroupsApi.selection.on.rowSelectionChanged($scope, function (row) {
                console.log(row);  // This is a nice option. It allowed my to browse the object and discover that I wanted the entity property.
                $scope.selectedGroups.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.selectedGroups.push(row.entity);
            });
        };

        // ---------------------------------------------------------------------------------------------------

        $scope.addGroup = function () {
            var modalInstance = $uibModal.open({
                templateUrl: 'partials/variableGroupsPicker.html',
                controller: 'groupsPickerModalController',
                size: 'sm',
                windowClass: 'app-modal-window'
            });

            modalInstance.result.then(function (overrides) {
                for (var i = 0; i < overrides.length; i++) {
                    $scope.server.CustomVariableGroups.push(overrides[i]);
                }
                $scope.saveServer();
            }, function () {
                // modal dismissed
            });
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.deleteGroups = function () {
            showConfirmationModal.show('Delete selected groups?', deleteGroups);
        }

        var deleteGroups = function (confirmed) {
            if (!confirmed) { return; }

            // Remove the selected items from the main list.
            $scope.server.CustomVariableGroups = $scope.server.CustomVariableGroups.filter(function (element) {
                return $scope.selectedGroups.indexOf(element) < 0;
            });

            $scope.saveServer();
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.saveServer = function () {
            var config = {
                url: '/PrestoWeb/api/server/save',
                method: 'POST',
                data: $scope.server
            };

            $http(config)
                .then(function (response) {
                    $scope.server = response.data;
                    $rootScope.setUserMessage("Server saved.");
                }, function (response) {
                    $rootScope.setUserMessage("Save failed");
                    console.log(response);
                    showInfoModal.show(response.statusText, response.data);
                });
        }
        // ---------------------------------------------------------------------------------------------------

        $scope.install = function () {
            var entityContainer = {
                server: $scope.server,
                appWithGroup: $scope.selectedAppsWithGroup[0]
            }

            var config = {
                url: '/PrestoWeb/api/server/installapp',
                method: 'POST',
                data: entityContainer
            };

            $http(config)
                .then(function (response) {
                    $rootScope.setUserMessage("Install request sent successfully");
                }, function (response) {
                    $rootScope.setUserMessage("Install request failed");
                    console.log(response);
                    showInfoModal.show(response.statusText, response.data);
                });
        };
    }

})();