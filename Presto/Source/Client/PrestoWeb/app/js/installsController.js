(function () {

    'use strict';

    angular.module('myApp.controllers').controller('installsController', installsController);

    // ------------------------------- Installs Controller -------------------------------    

    function installsController($rootScope, $scope, $http, $uibModal, installsRepository, pendingInstallsRepository, showInfoModal) {
        $scope.state = installsRepository;

        // ---------------------------------------------------------------------------------------------------

        // Whenever the user provides a new date, set the hours component on it.

        $scope.$watch('state.dateEnd', function () {
            $scope.state.dateEnd.setHours(23, 59, 59, 999); // Use ending of end date. (hour,min,sec,millisec)
        });

        // ---------------------------------------------------------------------------------------------------

        // ToDo: This grid should normally have nothing in it, so we should be able to collapse it.
        $scope.gridPending = {
            data: 'pending',
            multiSelect: false,
            enableColumnResizing: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            columnDefs: [{ field: 'ApplicationServer.Name', displayName: 'Server', width: "20%", resizable: true },
                         { field: 'ApplicationWithOverrideGroup.Application.Name', displayName: 'App', width: "35%" },
                         { field: 'ApplicationWithOverrideGroup.CustomVariableGroupNames', displayName: 'Groups', width: "43%" }]                         
        };

        $scope.gridOptions = {
            data: 'state.installs',
            multiSelect: false,
            enableColumnResizing: true,
            enableFiltering: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            columnDefs: [{ field: 'ApplicationName', displayName: 'App', width: "28%", resizable: true },
                         { field: 'ServerName', displayName: 'Server', width: "20%" },
                         { field: 'Environment', displayName: 'Env', width: "10%" },
                         { field: 'InstallationStart', displayName: 'Start', width: "15%" },
                         { field: 'InstallationEnd', displayName: 'End', width: "15%" },
                         { field: 'Result', displayName: 'Result', width: "10%" }]
        };

        // Act on the row selection changing.
        $scope.gridOptions.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                console.log(row);  // This is a nice option. It allowed my to browse the object and discover that I wanted the entity property.
                $scope.state.selectedSummaries.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.state.selectedSummaries.push(row.entity);
                $scope.state.selectedSummaryTaskDetails = row.entity.TaskDetails;
                $scope.state.selectedDetails.length = 0; // The details no longer match what is selected, so clear them.
            });
        };

        $scope.gridOptions2 = {
            //data: 'state.selectedSummaries[0].TaskDetails',
            data: 'state.selectedSummaryTaskDetails',
            multiSelect: false,
            enableColumnResizing: true,
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
                $scope.state.selectedDetails.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.state.selectedDetails.push(row.entity);
            });
        };

        $scope.refresh = function (forceRefresh) {
            if (!forceRefresh && $scope.state.installs.length > 0) {
                return; // Not forcing a refresh and we already have data.
            }

            var weekAgo = new Date();
            weekAgo.setDate(weekAgo.getDate() - 7);

            var appAndServerAndOverrides = {
                application: $scope.state.selectedApp,
                server: $scope.state.selectedServer,
                overrides: null,
                endDate: $scope.state.dateEnd
            }

            var config = {
                url: '/PrestoWeb/api/installs/',
                method: 'POST',
                data: appAndServerAndOverrides
            };

            $scope.loading = 1;
            $http(config)
                .then(function (result) {
                    $rootScope.setUserMessage("Installs list refreshed");
                    $scope.state.installs = result.data;
                    $scope.loading = 0;
                }, function (response) {
                    $scope.loading = 0;
                    showInfoModal.show(response.statusText, response.data);
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
                $scope.state.pending = dataResponse;
            });
        };

        $scope.refresh();

        // ---------------------------------------------------------------------------------------------------

        $scope.pickApp = function () {
            var modalInstance = $uibModal.open({
                templateUrl: 'partials/appPicker.html',
                controller: 'appPickerModalController',
                size: 'sm',
                windowClass: 'app-modal-window'
            });

            modalInstance.result.then(function (app) {
                console.log("App picked", app);
                $scope.state.selectedApp = app;
                clearInstalls();
            }, function () {
                // modal dismissed
            });
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.pickServer = function () {
            var modalInstance = $uibModal.open({
                templateUrl: 'partials/serverPicker.html',
                controller: 'serverPickerModalController',
                size: 'sm',
                windowClass: 'app-modal-window'
            });

            modalInstance.result.then(function (server) {
                console.log("Server picked", server);
                $scope.state.selectedServer = server;
                clearInstalls();
            }, function () {
                // modal dismissed
            });
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.clearApp = function () {
            $scope.state.selectedApp = null;
            clearInstalls();
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.clearServer = function () {
            $scope.state.selectedServer = null;
            clearInstalls();
        }

        // ---------------------------------------------------------------------------------------------------

        var clearInstalls = function () {
            $scope.state.installs.length = 0;            
            $scope.state.selectedSummaries.length = 0;
            $scope.state.selectedSummaryTaskDetails.length = 0;
            $scope.state.selectedDetails.length = 0;
        }

        // ---------------------------------------------------------------------------------------------------
        // Datepicker stuff

        $scope.popup1 = {
            opened: false
        };

        $scope.open1 = function () {
            $scope.popup1.opened = true;
        };
    }

})();