(function () {

    'use strict';

    angular.module('myApp.controllers').controller('groupsPickerModalController', groupsPickerModalController);

    function groupsPickerModalController($rootScope, $scope, $http, $uibModalInstance, uiGridConstants, variableGroupsRepository) {
        $scope.groups = null;

        $scope.gridGroups = {
            data: 'groups',
            multiSelect: true,
            enableColumnResizing: true,
            enableFiltering: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            selectedItems: $scope.selectedApps,
            columnDefs: [{ field: 'Name', displayName: 'Variable Group', width: "68%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 }, filter: { condition: uiGridConstants.filter.CONTAINS } }]
        };

        $scope.refresh = function () {
            $scope.loading = 1;
            $http.get('/PrestoWeb/api/variableGroups/')
                .then(function (result) {
                    $scope.loading = 0;
                    $scope.groups = result.data;
                    $rootScope.setUserMessage("Variable group list refreshed");                    
                });
        };

        // Act on the row selection changing.
        $scope.gridGroups.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
        };

        $scope.ok = function () {
            $uibModalInstance.close($scope.gridApi.selection.getSelectedRows());
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss();
        };

        $scope.refresh();
    };
})();