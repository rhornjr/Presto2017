(function () {

    'use strict';

    angular.module('myApp.controllers').controller('variableGroupController', variableGroupController);

    // ------------------------------- Variable Group Controller -------------------------------

    function variableGroupController($scope, $rootScope, $http, $routeParams, uiGridConstants, showInfoModal, $uibModal, showConfirmationModal, $window) {
        $scope.group = {};
        $scope.selectedVariables = [];
        $scope.variables = [];
        var lastSelectedVariable = null;

        // ---------------------------------------------------------------------------------------------------

        $scope.gridVariables = {
            data: 'variables',
            multiSelect: true,
            modifierKeysToMultiSelect : true, // must use shift/ctrl to select multiple rows.
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
                
                // A single click always happens during a double-click event. And apparently it's not trivial
                // to implement double-click and pass the selected row. So, when a single click occurs, set
                // the selected item. And for the double-click part, just call the edit method.
                if (rows.length > 0) {
                    lastSelectedVariable = rows[0];
                }

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
            var indexOfVariableBeingEdited = $scope.group.CustomVariables.indexOf(lastSelectedVariable);

            var modalInstance = $uibModal.open({
                templateUrl: 'partials/variable.html',
                controller: 'variableModalController',
                size: 'lg',
                resolve: {
                    variable: function () {
                        return lastSelectedVariable;
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

        // ---------------------------------------------------------------------------------------------------

        $scope.exportVariables = function () {
            var config = {
                url: '/PrestoWeb/api/variableGroups/getVariableExportFileContents',
                method: 'POST',
                data: $scope.selectedVariables
            };

            $scope.loading = 1;
            $http(config)
                .then(function (response) {
                    $scope.loading = 0;
                    // http://stackoverflow.com/a/33635761/279516
                    var blob = new Blob([response.data], { type: "text/plain" });
                    saveAs(blob, $scope.group.Name + '_variables.txt');
                }, function (response) {
                    $scope.loading = 0;
                    showInfoModal.show(response.statusText, response.data);
                    console.log(response);
                });
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.importVariables = function (fileContents) {
            var config = {
                url: '/PrestoWeb/api/variableGroups/importVariables',
                method: 'POST',
                data: { customVariableGroup: $scope.group, variablesAsString: fileContents }
            };

            $scope.loading = 1;
            $http(config)
                .then(function (response) {
                    $scope.loading = 0;
                    $scope.group = response.data;
                    $scope.variables = $scope.group.CustomVariables;
                    $scope.gridVariables.data = $scope.variables;
                }, function (response) {
                    $scope.loading = 0;
                    showInfoModal.show(response.statusText, response.data);
                    console.log(response);
                });
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.backToList = function () {
            $window.location.href = '/PrestoWeb/app/#/variableGroups/1';
        }

        // ---------------------------------------------------------------------------------------------------
    }
})();