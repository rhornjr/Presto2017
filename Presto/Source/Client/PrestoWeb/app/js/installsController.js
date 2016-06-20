(function () {

    'use strict';

    angular.module('myApp.controllers').controller('installsController', installsController);

    // ------------------------------- Installs Controller -------------------------------    

    function installsController($scope, $http, installsRepository, pendingInstallsRepository) {
        $scope.loading = 1;
        $scope.installs = null;
        $scope.pending = null;
        $scope.selectedSummaries = [];
        $scope.selectedDetails = [];

        // ToDo: This grid should normally have nothing in it, so we should be able to collapse it.
        $scope.gridPending = {
            data: 'pending',
            multiSelect: false,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            columnDefs: [{ field: 'ApplicationServer.Name', displayName: 'Server', width: "20%", resizable: true },
                         { field: 'ApplicationWithOverrideGroup.Application.Name', displayName: 'App', width: "35%" },
                         { field: 'ApplicationWithOverrideGroup.CustomVariableGroupNames', displayName: 'Groups', width: "43%" }]                         
        };

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
                console.log(row);  // This is a nice option. It allowed me to browse the object and discover that I wanted the entity property.
                $scope.selectedDetails.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.selectedDetails.push(row.entity);
            });
        };

        $scope.refresh = function (forceRefresh) {
            $scope.loading = 1;

            // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.
            installsRepository.getInstalls(forceRefresh, function (dataResponse) {
                $scope.installs = dataResponse;
                $scope.loading = 0;
            });

            pendingInstallsRepository.getPending(forceRefresh, function (dataResponse) {
                // Group names aren't showing. I believe it's because it's a calculated field and that doesn't run in a browser.
                // So let's at least show the first one here.
                for (var i = 0; i < dataResponse.length; i++)
                {
                    if (dataResponse[i].ApplicationWithOverrideGroup.CustomVariableGroups
                        && dataResponse[i].ApplicationWithOverrideGroup.CustomVariableGroups.length > 0) {
                        dataResponse[i].ApplicationWithOverrideGroup.CustomVariableGroupNames =
                            dataResponse[i].ApplicationWithOverrideGroup.CustomVariableGroups[0].Name + '...';
                    }
                }
                $scope.pending = dataResponse;
            });
        };

        $scope.refresh();
    }

})();