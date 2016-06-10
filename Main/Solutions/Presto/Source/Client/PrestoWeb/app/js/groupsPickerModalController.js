(function () {

    'use strict';

    angular.module('myApp.controllers').controller('groupsPickerModalController', groupsPickerModalController);

    function groupsPickerModalController ($scope, $uibModalInstance, uiGridConstants, variableGroupsRepository) {
        $scope.loading = 1;
        $scope.groups = null;

        $scope.gridGroups = {
            data: 'groups',
            multiSelect: true,
            enableFiltering: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            selectedItems: $scope.selectedApps,
            columnDefs: [{ field: 'Name', displayName: 'Variable Group', width: "68%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 }, filter: { condition: uiGridConstants.filter.CONTAINS } }]
        };

        $scope.refresh = function (forceRefresh) {
            $scope.loading = 1;
            // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.
            variableGroupsRepository.getVariableGroups(forceRefresh, function (dataResponse) {
                $scope.groups = dataResponse;
                $scope.loading = 0;
            });
        };

        // Act on the row selection changing.
        $scope.gridGroups.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
        };

        $scope.ok = function () {
            //$uibModalInstance.close($scope.selectedGroups);
            $uibModalInstance.close($scope.gridApi.selection.getSelectedRows());
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss();
        };

        $scope.refresh(true);
    };
})();