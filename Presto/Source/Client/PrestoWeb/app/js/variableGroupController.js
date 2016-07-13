(function () {

    'use strict';

    angular.module('myApp.controllers').controller('variableGroupController', variableGroupController);

    // ------------------------------- Variable Group Controller -------------------------------

    function variableGroupController($scope, $rootScope, $http, $routeParams, uiGridConstants, showInfoModal, $uibModal, showConfirmationModal) {
        $scope.group = {};
        $scope.selectedVariables = [];
        $scope.variables = [];

        // ---------------------------------------------------------------------------------------------------

        $scope.gridVariables = {
            data: 'variables',
            multiSelect: true,
            enableColumnResizing: true,
            enableFiltering: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            selectedItems: $scope.selectedVariables,
            columnDefs: [{ field: 'Key', displayName: 'Key', width: "30%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 }, filter: { condition: uiGridConstants.filter.CONTAINS } },
                         { field: 'Value', displayName: 'Value', width: "68%", sort: { direction: uiGridConstants.ASC, priority: 2 } }]
        };

        // ---------------------------------------------------------------------------------------------------

        // Act on the row selection changing.
        $scope.gridVariables.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                var rows = gridApi.selection.getSelectedRows();
                $scope.selectedVariables.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                for (var i = 0; i < rows.length; i++) {
                    $scope.selectedVariables.push(rows[i]);
                }                
            });
        };

        // ---------------------------------------------------------------------------------------------------

        $scope.saveVariableGroup = function () {
            var config = {
                url: '/PrestoWeb/api/variableGroups/save',
                method: 'POST',
                data: $scope.group
            };

            $scope.loading = 1;
            $http(config)
                .then(function (response) {
                    $scope.group = response.data;
                    $scope.variables = $scope.group.CustomVariables;
                    $rootScope.setUserMessage("Variable group saved.");
                    $scope.loading = 0;
                }, function (response) {
                    $scope.loading = 0;
                    $rootScope.setUserMessage("Save failed");
                    showInfoModal.show(response.statusText, response.data);
                });
        }

        // ---------------------------------------------------------------------------------------------------

        if ($routeParams.groupId) {
            $scope.loading = 1;
            $http.get('/PrestoWeb/api/variableGroups/' + $routeParams.groupId)
                .then(function (response) {
                    $scope.group = response.data;
                    $scope.variables = $scope.group.CustomVariables;
                    $scope.loading = 0;
                },
                function (response) {
                    $scope.loading = 0;
                    showInfoModal.show(response.statusText, response.data);
                });
        }
        else {
            // This is a new variable. Save it so we actually have a group when adding/editing/deleting
            // variables in it. If we don't save it here, we get an error when modifying variables.
            $scope.group.Name = 'Name' + new Date().valueOf();; // Give it a default name first.
            $scope.saveVariableGroup();
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.addVariable = function () {
            var modalInstance = $uibModal.open({
                templateUrl: 'partials/variable.html',
                controller: 'variableModalController',
                size: 'lg',
                resolve: {
                    variable: function () {
                        return null;
                    }
                }
            });

            modalInstance.result.then(function (variable) {
                $scope.group.CustomVariables.push(variable);
                $scope.saveVariableGroup();
            }, function () {
                // modal dismissed
            });
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.editVariable = function () {
            // Get the index of the selected item.
            var indexOfVariableBeingEdited = $scope.group.CustomVariables.indexOf($scope.selectedVariables[0]);

            var modalInstance = $uibModal.open({
                templateUrl: 'partials/variable.html',
                controller: 'variableModalController',
                size: 'lg',
                resolve: {
                    variable: function () {
                        return $scope.selectedVariables[0];
                    }
                }
            });

            modalInstance.result.then(function (variable) {
                $scope.group.CustomVariables[indexOfVariableBeingEdited] = variable;
                $scope.saveVariableGroup();
            }, function () {
                // modal dismissed
            });
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.removeVariables = function () {
            showConfirmationModal.show('Delete selected variables?', removeVariables);
        }

        var removeVariables = function (confirmed) {
            if (!confirmed) { return; }

            // Remove the selected items from the main list.
            $scope.group.CustomVariables = $scope.group.CustomVariables.filter(function (element) {
                return $scope.selectedVariables.indexOf(element) < 0;
            });

            $scope.saveVariableGroup();
        }
    }
})();