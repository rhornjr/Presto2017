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

    function appsController($scope, $rootScope, $uibModal, $http, $routeParams, appsRepository, $window, uiGridConstants) {
        if ($routeParams.showList == 1) {
            alert('show list');
        }
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
            $scope.loading = 1;
            // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.
            appsRepository.getApps(forceRefresh, function (dataResponse) {
                $scope.apps = dataResponse;
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
                var modalInstance = $uibModal.open({
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

        var saveApp = function (app) {
            $scope.loading = 1;

            var config = {
                url: '/PrestoWeb/api/app/saveApplication',
                method: 'POST',
                data: app
            };

            $http(config)
                .then(function (response) {
                    onAppSaved(app)
                }, function (response) {
                    $scope.loading = 0;
                    $rootScope.setUserMessage(app.Name + ' save failed.');
                    console.log(response);
                });
        }

        var onAppSaved = function (app) {
            $scope.refresh(true);
            $scope.loading = 0;
            $rootScope.userMessage = app.Name + ' saved.';
        }

        $scope.refresh(false);
    }        

})();