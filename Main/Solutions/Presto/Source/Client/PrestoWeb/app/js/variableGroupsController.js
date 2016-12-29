(function () {

    'use strict';

    angular.module('myApp.controllers').controller('variableGroupsController', variableGroupsController);

    // ------------------------------- Variable Groups Controller -------------------------------

    function variableGroupsController($scope, $rootScope, $http, $routeParams, variableGroupsState, $window, showConfirmationModal, showInfoModal, showTextEntryModal) {
        $scope.state = variableGroupsState;        
        var lastSelectedGroup = null;

        // Note: This is how you can see the filter value: $scope.state.gridVariableGroups.columnDefs[0].filter.term

        // ---------------------------------------------------------------------------------------------------

        $scope.refresh = function (forceRefresh) {
            if (!forceRefresh) {
                return;
            }

            $scope.loading = 1;
            $http.get('/PrestoWeb/api/variableGroups/')
                .then(function (result) {
                    $rootScope.setUserMessage("Variable group list refreshed");
                    $scope.state.variableGroups = result.data;
                    $scope.loading = 0;
                }, function (response) {
                    $scope.loading = 0;
                    console.log(response);
                    showInfoModal.show(response.statusText, response.data);
                });
        };

        // ---------------------------------------------------------------------------------------------------

        $scope.state.gridVariableGroups.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                console.log(row);  // This is a nice option. It allowed me to browse the object and discover that I wanted the entity property.
                $scope.state.selectedGroups.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.state.selectedGroups.push(row.entity);
                // A single click always happens during a double-click event. And apparently it's not trivial
                // to implement double-click and pass the selected row. So, when a single click occurs, set
                // the selected item. And for the double-click part, just call the edit method.
                if ($scope.state.selectedGroups.length > 0) {
                    lastSelectedGroup = $scope.state.selectedGroups[0];
                }
            });
        };

        // ---------------------------------------------------------------------------------------------------

        $scope.addGroup = function () {
            $window.location.href = '/PrestoWeb/app/#/variableGroup/';
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.editGroup = function () {
            var modifiedGroupId = $scope.state.selectedGroups[0].Id.replace("/", "^^");  // Because we shouldn't send slashes in a URL.
            $window.location.href = '/PrestoWeb/app/#/variableGroup/' + modifiedGroupId;
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.deleteGroup = function () {
            showConfirmationModal.show('Delete selected group?', deleteGroup);
        }

        var deleteGroup = function (confirmed) {
            if (!confirmed) { return; }
            var config = {
                url: '/PrestoWeb/api/variableGroups/delete',
                method: 'POST',
                data: $scope.state.selectedGroups[0]
            };

            $scope.loading = 1;
            $http(config)
                .then(function (response) {
                    $rootScope.setUserMessage("Variable group deleted.");
                    $scope.loading = 0;
                    $scope.refresh(true);
                }, function (response) {
                    $scope.loading = 0;
                    $rootScope.setUserMessage("Delete failed");
                    showInfoModal.show(response.statusText, response.data);
                });
        }

        // ---------------------------------------------------------------------------------------------------

        if ($scope.state.variableGroups.length == 0 || $routeParams.showList == 1) {
            $scope.state.selectedGroups.length = 0; // No longer have a selected group.
            $scope.refresh(true);
        }
        else {
            // If a group has been selected, go back to it.
            // Note: This is in a timeout because we can't redirect in the same turn as loading this page.
            //       The timeout callback happens in a different turn, so it works. If we called $scope.editGroup()
            //       directly (not in the timeout), we end up in an infinite loop calling this line.
            if ($scope.state.selectedGroups[0]) {
                setTimeout(function () {
                    $scope.editGroup();
                }, 100);
            }
        }        

        // ---------------------------------------------------------------------------------------------------

        $scope.findKey = function () {
            showTextEntryModal.show('Variable Key', onKeyEntered);
        }

        function onKeyEntered(key) {
            var filteredGroups = [];

            for (var i = 0; i < $scope.state.variableGroups.length; i++) {
                for (var j = 0; j < $scope.state.variableGroups[i].CustomVariables.length; j++) {
                    if ($scope.state.variableGroups[i].CustomVariables[j].Key == key) {
                        filteredGroups.push($scope.state.variableGroups[i]);
                    }
                }
            }

            $scope.state.variableGroups = filteredGroups;
        }
    }
})();