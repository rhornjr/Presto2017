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

    function appsController($scope, $rootScope, $uibModal, $http, $routeParams, appsRepository, appsState, $window, uiGridConstants) {
        $scope.state = appsState;
        var lastSelectedApp = null;

        $scope.state.gridOptions.data = $scope.state.filteredApps;

        $scope.refresh = function (forceRefresh) {
            if (!forceRefresh) {
                return;
            }
            $scope.loading = 1;
            appsRepository.getApps(forceRefresh, function (dataResponse) {
                $scope.state.allApps = dataResponse;

                filterAppsByArchived();
                $scope.state.gridOptions.data = $scope.state.filteredApps; // Grid doesn't update unless I do this. Not sure why.
                
                $scope.loading = 0;
                $rootScope.setUserMessage("Application list refreshed");
            });
        };

        // Act on the row selection changing.
        $scope.state.gridOptions.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                $scope.state.selectedApps.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.state.selectedApps.push(row.entity);
                // A single click always happens during a double-click event. And apparently it's not trivial
                // to implement double-click and pass the selected row. So, when a single click occurs, set
                // the selected item. And for the double-click part, just call the edit method.
                if ($scope.state.selectedApps.length > 0) {
                    lastSelectedApp = $scope.state.selectedApps[0];
                }
            });
        };

        $scope.editApp = function () {
            var modifiedAppId = $scope.state.selectedApps[0].Id.replace("/", "^^");  // Because we shouldn't send slashes in a URL.
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

        // If the apps haven't been loaded yet, or we've come here via a link telling us to load the list, then load the list.
        if ($scope.state.allApps.length == 0 || $routeParams.showList == 1) {
            $scope.state.selectedApps.length = 0; // No longer have a selected app.
            $scope.refresh(true);
        }
        else {
            // If an app has been selected, go back to it.
            // Note: This is in a timeout because we can't redirect in the same turn as loading this page.
            //       The timeout callback happens in a different turn, so it works. If we called $scope.editApp()
            //       directly (not in the timeout), we end up in an infinite loop calling this line.
            if ($scope.state.selectedApps[0]) {
                setTimeout(function () {
                    $scope.editApp();
                }, 100);
            }
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.toggleArchived = function () {
            $scope.state.showArchived = !$scope.state.showArchived;

            $scope.state.archiveButtonText = 'Show archived'; // default

            if ($scope.state.showArchived) {
                $scope.state.archiveButtonText = 'Hide archived';
            }

            filterAppsByArchived();
            $scope.state.gridOptions.data = $scope.state.filteredApps; // Grid doesn't update unless I do this. Not sure why.
        }

        // ---------------------------------------------------------------------------------------------------

        function filterAppsByArchived() {
            if (!$scope.state.allApps) {
                $scope.state.filteredApps = [];
                return;
            }

            if ($scope.state.showArchived) {
                $scope.state.filteredApps = $scope.state.allApps.filter(function (element) {
                    return true;
                });
                return;
            }
            
            $scope.state.filteredApps = $scope.state.allApps.filter(function (element) {
                if (element.Archived) {
                    return false;
                }
                return true;
            });
        }
    }        
})();