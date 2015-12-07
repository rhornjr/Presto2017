(function () {

    'use strict';

    angular.module('myApp.controllers').controller('appController', appController);

    // ------------------------------- App Controller -------------------------------

    function appController($scope, $http, $routeParams, uiGridConstants) {
        $scope.loading = 1;
        $scope.app = null;
        $scope.appId = $routeParams.appId;

        $scope.gridOptions = {
            multiSelect: false,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            // Got the row template from https://github.com/cdwv/ui-grid-draggable-rows:
            rowTemplate: '<div grid="grid" class="ui-grid-draggable-row" draggable="true"><div ng-repeat="(colRenderIndex, col) in colContainer.renderedColumns track by col.colDef.name" class="ui-grid-cell" ng-class="{ \'ui-grid-row-header-cell\': col.isRowHeader, \'custom\': true }" ui-grid-cell></div></div>',
            columnDefs: [{ field: 'Sequence', displayName: 'Order', type: 'number', width: "8%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 } },
                         { field: 'Description', displayName: 'Description', width: "62%" },
                         { field: 'PrestoTaskType', displayName: 'Type', width: "16%" },
                         { field: 'FailureCausesAllStop', displayName: 'Stop', width: "12%" }]
        };

        $scope.gridOptions.onRegisterApi = function (gridApi) {
            gridApi.draggableRows.on.rowDropped($scope, function (info, dropTarget) {
                console.log("Dropped", info);
            });
        };

        $scope.gridOptions2 = {
            data: 'app.CustomVariableGroups',
            multiSelect: false,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            columnDefs: [{ field: 'Name', displayName: 'Name', width: "98%", resizable: true }]
        };

        $http.get('/PrestoWeb/api/app/' + $scope.appId)
                  .then(function (result) {
                      $scope.app = result.data;
                      $scope.loading = 0;
                      console.log(result.data.Tasks);
                      $scope.gridOptions.data = result.data.Tasks;
                  },
                  function () {
                      $scope.loading = 0;
                      alert("An error occurred and the app could not be loaded.");
                  });
    }           

})();