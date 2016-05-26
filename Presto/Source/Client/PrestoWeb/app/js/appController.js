(function () {

    'use strict';

    angular.module('myApp.controllers').controller('appController', appController);

    // ------------------------------- Modal Controllers -------------------------------

    angular.module('myApp.controllers').controller('taskModalController', function ($scope, $uibModalInstance, task) {
        $scope.task = task;
        
        $scope.ok = function () {
            $uibModalInstance.close(task);
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss();
        };
    });

    // ------------------------------- App Controller -------------------------------

    function appController($scope, $rootScope, $http, $uibModal, $routeParams, uiGridConstants) {
        $scope.loading = 1;
        $scope.app = null;
        $scope.appId = $routeParams.appId;
        $scope.selectedTasks = [];

        // ToDo: After moving tasks, need to enable a save button. The tasks don't save as they're moved.

        $scope.moveTaskDown = function () {
            moveTask(1);
        }

        $scope.moveTaskUp = function () {
            moveTask(-1);
        }

        var moveTask = function (multiplier) {
            // The way to make sense of this method is to think of the multiplier as a 1, and that's
            // what works for moving a task down. Instead of trying to make sense of things like
            // "-= (1 * multiplier)" just realize that a multiplier of -1 does the opposite of 1.
            // It just works.
            // Get the index of the selected task.
            var selectedIndex = 0;
            for (var i = 0; i < $scope.gridOptions.data.length; i++) {
                if ($scope.gridOptions.data[i].Sequence == $scope.selectedTasks[0].Sequence) {
                    selectedIndex = i;
                    break;
                }
            }

            // Don't allow a move up if we're dealing with the top-most item already.
            if (selectedIndex == 0 && multiplier == -1) {
                return;
            }

            // Don't allow a move down if we're dealing with the bottom-most item already.
            if (selectedIndex == $scope.gridOptions.data.length - 1 && multiplier == 1) {
                return;
            }

            var taskToMove = $scope.gridOptions.data[selectedIndex];
            var taskToSwap = $scope.gridOptions.data[selectedIndex + (1 * multiplier)];

            taskToMove.Sequence += (1 * multiplier);
            taskToSwap.Sequence -= (1 * multiplier);

            $scope.gridOptions.data[selectedIndex] = taskToSwap;
            $scope.gridOptions.data[selectedIndex + (1 * multiplier)] = taskToMove;
        }

        $scope.gridOptions = {
            multiSelect: false,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            selectedItems: $scope.selectedTasks,
            columnDefs: [{ field: 'Sequence', displayName: 'Order', type: 'number', width: "8%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 } },
                         { field: 'Description', displayName: 'Description', width: "62%" },
                         { field: 'PrestoTaskType', displayName: 'Type', width: "16%" },
                         { field: 'FailureCausesAllStop', displayName: 'Stop', width: "12%" }]
        };

        $scope.gridOptions.onRegisterApi = function (gridApi) {
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
            openTask($scope.selectedTasks[0]);
        }

        var openTask = function (task) {
            var modalInstance = $uibModal.open({
                templateUrl: 'partials/task' + task.PrestoTaskType + '.html',
                controller: 'taskModalController',
                size: 'sm',
                windowClass: 'app-modal-window',
                resolve: {
                    task: function () {
                        return $scope.selectedTasks[0];  // pass data to modal
                    }
                }
            });

            modalInstance.result.then(function (task) {
                // Since task is a derived type and the Web API controller accepts the base
                // type TaskBase, we need to set the $type property on the object so it will deserialize
                // correctly in the Web API method. If we don't do this, the app.Tasks property has 0 items.
                task.$type = 'PrestoCommon.Entities.Task' + task.PrestoTaskType + ', PrestoCommon';

                var config = {
                    url: '/PrestoWeb/api/app/saveApplication',
                    method: 'POST',
                    data: $scope.app
                };

                $http(config)
                    .then(function (response) {
                        $rootScope.setUserMessage("App saved");
                    }, function (response) {
                        console.log(response);
                    });
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