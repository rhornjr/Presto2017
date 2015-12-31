(function () {

    'use strict';

    angular.module('myApp.controllers').controller('appController', appController);

    // ------------------------------- Modal Controllers -------------------------------

    angular.module('myApp.controllers').controller('taskDosCommandModalController', function ($scope, $uibModalInstance, task) {
        $scope.task = task;
        
        $scope.ok = function () {
            $uibModalInstance.close(task);
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss();
        };
    });

    // ------------------------------- App Controller -------------------------------

    function appController($scope, $http, $uibModal, $routeParams, uiGridConstants) {
        $scope.loading = 1;
        $scope.app = null;
        $scope.appId = $routeParams.appId;
        $scope.selectedTasks = [];

        $scope.gridOptions = {
            multiSelect: false,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            // Got the row template from https://github.com/cdwv/ui-grid-draggable-rows:
            // Note: Enabling rowTemplate (next line) caused the line to not be highlighted when clicked.
            //       I don't know if rowTemplate is necessary anymore. If we don't notice an issue, just delete it.
            //rowTemplate: '<div grid="grid" class="ui-grid-draggable-row" draggable="true"><div ng-repeat="(colRenderIndex, col) in colContainer.renderedColumns track by col.colDef.name" class="ui-grid-cell" ng-class="{ \'ui-grid-row-header-cell\': col.isRowHeader, \'custom\': true }" ui-grid-cell></div></div>',
            selectedItems: $scope.selectedTasks,
            columnDefs: [{ field: 'Sequence', displayName: 'Order', type: 'number', width: "8%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 } },
                         { field: 'Description', displayName: 'Description', width: "62%" },
                         { field: 'PrestoTaskType', displayName: 'Type', width: "16%" },
                         { field: 'FailureCausesAllStop', displayName: 'Stop', width: "12%" }]
        };

        $scope.gridOptions.onRegisterApi = function (gridApi) {
            // This caused an error after disabling rowTemplate in the grid above.
            //gridApi.draggableRows.on.rowDropped($scope, function (info, dropTarget) {
            //    console.log("Dropped", info);
            //});
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                $scope.selectedTasks.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.selectedTasks.push(row.entity);
            });
        };

        $scope.gridOptions2 = {
            data: 'app.CustomVariableGroups',
            multiSelect: false,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            columnDefs: [{ field: 'Name', displayName: 'Name', width: "98%", resizable: true }]
        };

        $scope.editTask = function() {
            console.log("Task: ", $scope.selectedTasks[0].PrestoTaskType);
            // Need to open the right page here, based on the task type (XmlModify, CopyFile, etc.)
            if ($scope.selectedTasks[0].PrestoTaskType == "DosCommand") { openDosCommand($scope.selectedTasks[0]) }
        }

        var openDosCommand = function (dosCommand) {
            var modalInstance = $uibModal.open({
                templateUrl: 'partials/taskDosCommand.html',
                controller: 'taskDosCommandModalController',
                size: 'sm',
                windowClass: 'app-modal-window',
                resolve: {
                    task: function () {
                        return $scope.selectedTasks[0];  // pass data to modal
                    }
                }
            });

            modalInstance.result.then(function (dosCommand) {
                console.log("taskDosCommand", dosCommand);
            }, function () {
                // modal dismissed
            });
        }

        $http.get('/PrestoWeb/api/app/' + $scope.appId)
            .then(function (result) {
                $scope.app = result.data;
                $scope.loading = 0;
                $scope.gridOptions.data = result.data.Tasks;
            },
            function () {
                $scope.loading = 0;
                alert("An error occurred and the app could not be loaded.");
            });
    }           

})();