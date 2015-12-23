(function () {

    'use strict';

    angular.module('myApp.controllers').controller('variableGroupsController', variableGroupsController);

    // ------------------------------- Variable Groups Controller -------------------------------

    function variableGroupsController($scope, $rootScope, $http, $routeParams, variableGroupsRepository, uiGridConstants) {
        $scope.loading = 1;
        $scope.variableGroups = null;

        $scope.gridVariableGroups = {
            data: 'variableGroups',
            multiSelect: false,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            // Got the row template from https://github.com/cdwv/ui-grid-draggable-rows:
            rowTemplate: '<div grid="grid" class="ui-grid-draggable-row" draggable="true"><div ng-repeat="(colRenderIndex, col) in colContainer.renderedColumns track by col.colDef.name" class="ui-grid-cell" ng-class="{ \'ui-grid-row-header-cell\': col.isRowHeader, \'custom\': true }" ui-grid-cell></div></div>',
            columnDefs: [{ field: 'Name', displayName: 'Name', width: "98%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 } }]
        };

        $scope.refresh = function (forceRefresh) {
            // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.
            variableGroupsRepository.getVariableGroups(forceRefresh, function (dataResponse) {
                $scope.variableGroups = dataResponse;
                $scope.loading = 0;
            });
        };

        $scope.gridVariableGroups.onRegisterApi = function (gridApi) {
            gridApi.draggableRows.on.rowDropped($scope, function (info, dropTarget) {
                console.log("Dropped", info);
            });
        };

        $scope.refresh();
    }

})();