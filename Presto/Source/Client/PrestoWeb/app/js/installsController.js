(function () {

    'use strict';

    angular.module('myApp.controllers').controller('installsController', installsController);

    // ------------------------------- Installs Controller -------------------------------

    function installRequestSucceeded(loading) {
        alert('Install request sent successfully.');
    }

    function installsController($scope, $http) {
        $scope.loading = 1;
        $scope.installs = null;
        $scope.selectedSummaries = [];
        $scope.selectedDetails = [];

        $scope.gridOptions = {
            data: 'installs',
            multiSelect: false,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            columnDefs: [{ field: 'ApplicationName', displayName: 'App', width: "28%", resizable: true },
                         { field: 'ServerName', displayName: 'Server', width: "20%" },
                         { field: 'InstallationStart', displayName: 'Start', width: "20%" },
                         { field: 'InstallationEnd', displayName: 'End', width: "20%" },
                         { field: 'Result', displayName: 'Result', width: "10%" }]
        };

        // Act on the row selection changing.
        $scope.gridOptions.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                console.log(row);  // This is a nice option. It allowed my to browse the object and discover that I wanted the entity property.
                $scope.selectedSummaries.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.selectedSummaries.push(row.entity);
                $scope.selectedDetails.length = 0; // The details no longer match what is selected, so clear them.
            });
        };

        $scope.gridOptions2 = {
            data: 'selectedSummaries[0].TaskDetails',
            multiSelect: false,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            columnDefs: [{ field: 'StartTime', displayName: 'Start', width: "20%", resizable: true },
                         { field: 'EndTime', displayName: 'End', width: "20%" },
                         { field: 'Details', displayName: 'Details', width: "58%" }]
        };

        // Act on the row selection changing.
        $scope.gridOptions2.onRegisterApi = function (gridApi) {
            $scope.gridApi2 = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                console.log(row);  // This is a nice option. It allowed my to browse the object and discover that I wanted the entity property.
                $scope.selectedDetails.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.selectedDetails.push(row.entity);
            });
        };

        $http.get('/PrestoWeb/api/installs/')
              .then(function (result) {
                  $scope.installs = result.data;
                  $scope.loading = 0;
              });
    }

})();