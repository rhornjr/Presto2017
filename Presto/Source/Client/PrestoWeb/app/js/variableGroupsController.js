(function () {

    'use strict';

    angular.module('myApp.controllers').controller('variableGroupsController', variableGroupsController);

    // ------------------------------- Variable Groups Controller -------------------------------

    function variableGroupsController($scope, $rootScope, $http, $routeParams, variableGroupsRepository, uiGridConstants, $window, showConfirmationModal, showInfoModal, showTextEntryModal) {
        $scope.state = variableGroupsRepository;
        var lastSelectedGroup = null;

        // ---------------------------------------------------------------------------------------------------

        $scope.gridVariableGroups = {
            data: 'state.variableGroups',
            multiSelect: false,
            enableColumnResizing: true,
            enableFiltering: true,
            selectedItems: $scope.state.selectedGroups,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            columnDefs: [{ field: 'Name', displayName: 'Name', width: "98%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 } }]
        };

        // ---------------------------------------------------------------------------------------------------

        $scope.refresh = function (forceRefresh) {
            if ($scope.state.variableGroups.length > 0 && !forceRefresh) {
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

        $scope.gridVariableGroups.onRegisterApi = function (gridApi) {
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

        $scope.refresh();

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