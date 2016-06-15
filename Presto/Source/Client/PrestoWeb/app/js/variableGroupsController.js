(function () {

    'use strict';

    angular.module('myApp.controllers').controller('variableGroupsController', variableGroupsController);

    // ------------------------------- Variable Groups Controller -------------------------------

    function variableGroupsController($scope, $rootScope, $http, $routeParams, variableGroupsRepository, uiGridConstants, $window) {
        $scope.loading = 1;
        $scope.variableGroups = null;
        $scope.selectedGroups = [];

        $scope.gridVariableGroups = {
            data: 'variableGroups',
            multiSelect: false,
            enableFiltering: true,
            selectedItems: $scope.selectedGroups,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            columnDefs: [{ field: 'Name', displayName: 'Name', width: "98%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 } }]
        };

        $scope.refresh = function (forceRefresh) {
            $scope.loading = 1;
            // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.
            variableGroupsRepository.getVariableGroups(forceRefresh, function (dataResponse) {
                $scope.variableGroups = dataResponse;
                $scope.loading = 0;
            });
        };

        $scope.gridVariableGroups.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                console.log(row);  // This is a nice option. It allowed me to browse the object and discover that I wanted the entity property.
                $scope.selectedGroups.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.selectedGroups.push(row.entity);
                });
        };

        $scope.editGroup = function () {
            var modifiedGroupId = $scope.selectedGroups[0].Id.replace("/", "^^");  // Because we shouldn't send slashes in a URL.
            $window.location.href = '/PrestoWeb/app/#/variableGroup/' + modifiedGroupId;
        }

        $scope.refresh();
    }
})();