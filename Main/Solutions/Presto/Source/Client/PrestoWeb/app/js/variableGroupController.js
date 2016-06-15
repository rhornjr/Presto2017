(function () {

    'use strict';

    angular.module('myApp.controllers').controller('variableGroupController', variableGroupController);

    // ------------------------------- Variable Group Controller -------------------------------

    function variableGroupController($scope, $rootScope, $http, $routeParams, uiGridConstants, showInfoModal) {
        $scope.group = {};
        $scope.selectedVariables = [];
        $scope.variables = [];

        // ---------------------------------------------------------------------------------------------------

        $scope.gridVariables = {
            data: 'variables',
            multiSelect: false,
            enableFiltering: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            selectedItems: $scope.selectedVariables,
            columnDefs: [{ field: 'Key', displayName: 'Key', width: "30%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 }, filter: { condition: uiGridConstants.filter.CONTAINS } },
                         { field: 'Value', displayName: 'Value', width: "68%", sort: { direction: uiGridConstants.ASC, priority: 2 } }]
        };

        // ---------------------------------------------------------------------------------------------------

        // Act on the row selection changing.
        $scope.gridVariables.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                $scope.selectedVariables.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.selectedVariables.push(row.entity);
            });
        };

        // ---------------------------------------------------------------------------------------------------

        $http.get('/PrestoWeb/api/variableGroups/' + $routeParams.groupId)
            .then(function (response) {
                $scope.group = response.data;
                $scope.variables = $scope.group.CustomVariables;
                $scope.loading = 0;
            },
            function (response) {
                $scope.loading = 0;
                showInfoModal.show(response.statusText, response.data);
            });
    }
})();