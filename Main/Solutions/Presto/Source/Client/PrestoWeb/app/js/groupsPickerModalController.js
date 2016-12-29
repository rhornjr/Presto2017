(function () {

    'use strict';

    angular.module('myApp.controllers').controller('groupsPickerModalController', groupsPickerModalController);

    function groupsPickerModalController($rootScope, $scope, $http, $uibModalInstance, $timeout, uiGridConstants, selectedOverrides) {
        $scope.groups = null;

        $scope.gridGroups = {
            data: 'groups',
            multiSelect: true,
            enableColumnResizing: true,
            enableFiltering: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            selectedItems: $scope.selectedApps,
            columnDefs: [{ field: 'Name', displayName: 'Variable Group', width: "98%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 }, filter: { condition: uiGridConstants.filter.CONTAINS } }]
        };

        $scope.refresh = function () {
            $scope.loading = 1;
            $http.get('/PrestoWeb/api/variableGroups/')
                .then(function (result) {
                    $scope.loading = 0;
                    $scope.groups = result.data;
                    $rootScope.setUserMessage("Variable group list refreshed");
                    $timeout(function () {
                        setSelectedItems();
                        // Use class hierarchy to put the focus on the filter in the grid.
                        var elements = document.querySelector('.modal .ui-grid-header-cell .ui-grid-filter-input-0');
                        elements.focus();
                    }, 50);
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

        function setSelectedItems() {
            if (!selectedOverrides) {
                return;
            }

            // Loop through the selected overrides passed into this modal. Find the group and select it.
            for (var i = 0; i < selectedOverrides.length; i++) {
                for (var j = 0; j < $scope.groups.length; j++) {
                    if ($scope.groups[j].Id == selectedOverrides[i].Id) {
                        $scope.gridApi.selection.selectRow($scope.groups[j]);
                        break;
                    }
                }
            }
        }

        $scope.refresh();
    };
})();