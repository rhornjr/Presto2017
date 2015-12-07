(function () {

    'use strict';

    angular.module('myApp.controllers').controller('appsController', appsController)

    angular.module('ui.grid.draggable-rows', ['ui.grid']);

    // ------------------------------- Modal Controllers -------------------------------

    angular.module('myApp.controllers').controller('appAddModalController', function ($scope, $modalInstance) {
        console.log('In appAddModalController');
        $scope.name = '';
        $scope.version = '';

        $scope.ok = function () {
            $modalInstance.close({ Name: $scope.name, Version: $scope.version });
        };

        $scope.cancel = function () {
            $modalInstance.dismiss();
        };
    });

    // ------------------------------- Apps Controller -------------------------------

    function appsController($scope, $modal, $http, appsRepository, $window, uiGridConstants) {
        $scope.loading = 1;
        $scope.apps = null;
        $scope.selectedApps = [];

        $scope.gridOptions = {
            data: 'apps',
            multiSelect: false,
            enableFiltering: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            selectedItems: $scope.selectedApps,
            columnDefs: [{ field: 'Name', displayName: 'Application', width: "78%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 }, filter: {condition: uiGridConstants.filter.CONTAINS} },
                         { field: 'Version', displayName: 'Version', width: "20%", sort: { direction: uiGridConstants.ASC, priority: 2 } }]
        };

        $scope.refresh = function (forceRefresh) {
            // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.
            appsRepository.getApps(forceRefresh, function (dataResponse, lastRefreshTime) {
                $scope.apps = dataResponse;
                $scope.lastRefreshTime = lastRefreshTime;
                $scope.loading = 0;
            });
        };

        // Act on the row selection changing.
        $scope.gridOptions.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                console.log(row);  // This is a nice option. It allowed me to browse the object and discover that I wanted the entity property.
                $scope.selectedApps.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.selectedApps.push(row.entity);
            });
        };

        $scope.editApp = function () {
            var modifiedAppId = $scope.selectedApps[0].Id.replace("/", "^^");  // Because we shouldn't send slashes in a URL.
            $window.location.href = '/PrestoWeb/app/#/app/' + modifiedAppId;
        };

        $scope.addApp = function () {
            console.log("addApp() called.");
                var modalInstance = $modal.open({
                    templateUrl: 'partials/appAdd.html',
                    controller: 'appAddModalController',
                    size: 'sm',
                    //windowClass: 'modalConfirmationPosition'
                    windowClass: 'app-modal-window'
                });

                modalInstance.result.then(function (app) {
                    console.log("App", app);
                    saveApp(app);
                }, function () {
                    // modal dismissed
                });
        }

        var saveApp = function(app) {
            $.ajax({
                url: '/PrestoWeb/api/app/saveApplication',
                type: 'POST',
                data: JSON.stringify(app),
                contentType: "application/json",
                success: onAppSaved(app),
                error: function (jqXHR, textStatus, errorThrown) {
                    console.log("App save failed.");
                }
            });
        }

        var onAppSaved = function(app) {
            alert('app saved');
        }

        $scope.refresh(false);
    }        

})();