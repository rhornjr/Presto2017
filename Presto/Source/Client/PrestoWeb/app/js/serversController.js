(function () {

    'use strict';

    angular.module('myApp.controllers').controller('serversController', serversController);

    // ------------------------------- Servers Controller -------------------------------

    function serversController($scope, $rootScope, $routeParams, serversRepository, serversState, $window, uiGridConstants) {
        $scope.state = serversState;
        var lastSelectedServer = null;

        $scope.state.gridOptions.data = $scope.state.filteredServers;

        $scope.refresh = function (forceRefresh) {
            $scope.loading = 1;
            // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.          
            serversRepository.getServers(forceRefresh, function (dataResponse) {
                $scope.state.allServers = dataResponse;
                filterServersByArchived();
                $scope.state.gridOptions.data = $scope.state.filteredServers;
                $scope.loading = 0;
                $rootScope.setUserMessage("Server list refreshed");
            });
        };

        // Act on the row selection changing.
        $scope.state.gridOptions.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                console.log(row);  // This is a nice option. It allowed me to browse the object and discover that I wanted the entity property.
                $scope.state.selectedServers.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.state.selectedServers.push(row.entity);
                // A single click always happens during a double-click event. And apparently it's not trivial
                // to implement double-click and pass the selected row. So, when a single click occurs, set
                // the selected item. And for the double-click part, just call the edit method.
                if ($scope.state.selectedServers.length > 0) {
                    lastSelectedServer = $scope.state.selectedServers[0];
                }
            });
        };

        $scope.addServer = function () {
            $window.location.href = '/PrestoWeb/app/#/server';
        };

        $scope.editServer = function () {
            var modifiedServerId = $scope.state.selectedServers[0].Id.replace("/", "^^");  // Because we shouldn't send slashes in a URL.
            $window.location.href = '/PrestoWeb/app/#/server/' + modifiedServerId;
        };

        // If the servers haven't been loaded yet, or we've come here via a link telling us to load the list, then load the list.
        if (!$scope.state.allServers || $routeParams.showList == 1) {
            $scope.state.selectedServers.length = 0; // No longer have a selected server.
            $scope.refresh(false);
        }
        else {
            // If a server has been selected, go back to it.
            // Note: This is in a timeout because we can't redirect in the same turn as loading this page.
            //       The timeout callback happens in a different turn, so it works. If we called $scope.editServer()
            //       directly (not in the timeout), we end up in an infinite loop calling this line.
            if ($scope.state.selectedServers[0]) {
                setTimeout(function () {
                    $scope.editServer();
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

            filterServersByArchived();
            $scope.state.gridOptions.data = $scope.state.filteredServers; // Grid doesn't update unless I do this. Not sure why.
        }

        // ---------------------------------------------------------------------------------------------------

        function filterServersByArchived() {
            if (!$scope.state.allServers) {
                $scope.state.filteredServers = [];
                return;
            }

            if ($scope.state.showArchived) {
                $scope.state.filteredServers = $scope.state.allServers.filter(function (element) {
                    return true;
                });
                return;
            }

            $scope.state.filteredServers = $scope.state.allServers.filter(function (element) {
                if (element.Archived) {
                    return false;
                }
                return true;
            });
        }
    }

})();