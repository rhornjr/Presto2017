(function () {

    'use strict';

    angular.module('myApp.controllers').controller('resolveController', resolveController);

    // ------------------------------- Modal Controllers -------------------------------

    angular.module('myApp.controllers').controller('appPickerModalController', function ($scope, $uibModalInstance, uiGridConstants, appsRepository) {
        $scope.loading = 1;
        $scope.apps = null;
        $scope.selectedApps = [];

        $scope.gridApps = {
            data: 'apps',
            multiSelect: false,
            enableFiltering: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            selectedItems: $scope.selectedApps,
            columnDefs: [{ field: 'Name', displayName: 'Application', width: "78%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 }, filter: { condition: uiGridConstants.filter.CONTAINS } },
                         { field: 'Version', displayName: 'Version', width: "20%", sort: { direction: uiGridConstants.ASC, priority: 2 } }]
        };

        $scope.refresh = function (forceRefresh) {
            $scope.loading = 1;
            // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.
            appsRepository.getApps(forceRefresh, function (dataResponse) {
                $scope.apps = dataResponse;
                $scope.loading = 0;
            });
        };

        // Act on the row selection changing.
        $scope.gridApps.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                console.log(row.entity);  // This is a nice option. It allowed me to browse the object and discover that I wanted the entity property.
                $scope.selectedApps.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.selectedApps.push(row.entity);
            });
        };

        $scope.ok = function () {
            $uibModalInstance.close($scope.selectedApps[0]);
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss();
        };

        $scope.refresh(true);
    });

    // ------------------------------- Resolve Controller -------------------------------

    function resolveController($scope, $http, $uibModal, uiGridConstants) {
        $scope.selectedApp;

        $scope.gridResolve = {
            data: 'variableValues',
            multiSelect: false,
            enableFiltering: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            selectedItems: $scope.selectedApps,
            columnDefs: [{ field: 'Name', displayName: 'Variable', width: "20%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 }, filter: { condition: uiGridConstants.filter.CONTAINS } },
                         { field: 'Version', displayName: 'Value', width: "78%", sort: { direction: uiGridConstants.ASC, priority: 2 } }]
        };

        $scope.pickApp = function () {
            var modalInstance = $uibModal.open({
                templateUrl: 'partials/appPicker.html',
                controller: 'appPickerModalController',
                size: 'sm',
                //windowClass: 'modalConfirmationPosition'
                windowClass: 'app-modal-window'
            });

            modalInstance.result.then(function (app) {
                console.log("App picked", app);
                $scope.selectedApp = app;
            }, function () {
                // modal dismissed
            });
        }
    }

})();