(function () {

    'use strict';

    angular.module('myApp.controllers').controller('appController', appController);

    // ------------------------------- Task Modal Controller -------------------------------

    angular.module('myApp.controllers').controller('taskModalController', function ($scope, $uibModalInstance, task) {
        $scope.task = task;
        
        $scope.ok = function () {
            $uibModalInstance.close(task);
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss();
        };
    });

    // ------------------------------- Task Type Selector Modal Controller -------------------------------

    angular.module('myApp.controllers').controller('taskTypeSelectorModalController', function ($scope, $uibModalInstance) {
        $scope.onTaskSelected = function (taskType) {
            $uibModalInstance.close(taskType);
        }

        $scope.cancel = function () {
            $uibModalInstance.dismiss();
        };
    });


    // ------------------------------- App Controller -------------------------------

    function appController($scope, $rootScope, $http, $uibModal, $routeParams, uiGridConstants, showInfoModal, showConfirmationModal) {
        $scope.loading = 1;
        $scope.app = null;
        $scope.appId = $routeParams.appId;
        $scope.selectedTasks = [];

        $scope.moveTaskDown = function () {
            moveTask(1);
            $scope.isDirty = true;
        }

        $scope.moveTaskUp = function () {
            moveTask(-1);
            $scope.isDirty = true;
        }

        // ---------------------------------------------------------------------------------------------------

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

            var taskToMove = $scope.gridOptions.data[selectedIndex];

            // Don't allow a move up if we're dealing with the top-most item already.
            if (taskToMove.Sequence == 1 && multiplier == - 1) {
                return;
            }

            // Don't allow a move down if we're dealing with the bottom-most item already.
            if (taskToMove.Sequence == $scope.gridOptions.data.length && multiplier == 1) {
                return;
            }

            // Get the sequence of the task to swap.
            var sequenceOfTaskToSwap = taskToMove.Sequence + (1 * multiplier);

            var indexOfTaskToSwap = 0;
            for (var i = 0; i < $scope.gridOptions.data.length; i++) {
                if ($scope.gridOptions.data[i].Sequence == sequenceOfTaskToSwap) {
                    indexOfTaskToSwap = i;
                    break;
                }
            }
            
            var taskToSwap = $scope.gridOptions.data[indexOfTaskToSwap];

            taskToMove.Sequence += (1 * multiplier);
            taskToSwap.Sequence -= (1 * multiplier);

            $scope.gridOptions.data[selectedIndex] = taskToSwap;
            $scope.gridOptions.data[indexOfTaskToSwap] = taskToMove;
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.exportTasks = function () {
            var config = {
                url: '/PrestoWeb/api/app/getTaskExportFileContents',
                method: 'POST',
                data: $scope.selectedTasks
            };

            $scope.loading = 1;
            $http(config)
                .then(function (response) {
                    $scope.loading = 0;
                    // http://stackoverflow.com/a/33635761/279516
                    var blob = new Blob([response.data], { type: "text/plain" });
                    saveAs(blob, 'snuh.txt');
                }, function (response) {
                        $scope.loading = 0;
                        showInfoModal.show(response.statusText, response.data);
                        console.log(response);
                    });
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.importTasks = function (fileContents) {
            var config = {
                url: '/PrestoWeb/api/app/importTasks',
                method: 'POST',
                data: { application: $scope.app, tasksAsString: fileContents }
            };

            $scope.loading = 1;
            $http(config)
                .then(function (response) {
                    $scope.loading = 0;
                    $scope.app = response.data;
                    $scope.gridOptions.data = $scope.app.Tasks;
                }, function (response) {
                    $scope.loading = 0;
                    showInfoModal.show(response.statusText, response.data);
                    console.log(response);
                });
        }
        
        // ---------------------------------------------------------------------------------------------------

        $scope.addTask = function () {
            // Show modal for user to pick task type to add
            var modalInstance = $uibModal.open({
                templateUrl: 'partials/taskTypeSelectorModal.html',
                controller: 'taskTypeSelectorModalController',
                size: 'sm',
                windowClass: 'app-modal-window'
            });

            modalInstance.result.then(function (taskType) {
                var task = {
                    PrestoTaskType: taskType
                }
                openTask(task);
            }, function () {
                // modal dismissed
            });
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.deleteTasks = function () {
            showConfirmationModal.show('Delete selected tasks?', deleteTasks);
        }

        var deleteTasks = function (confirmed) {
            if (!confirmed) { return; }

            // Remove the selected tasks from the main task list.
            $scope.app.Tasks = $scope.app.Tasks.filter(function (element) {
                return $scope.selectedTasks.indexOf(element) < 0;
            });

            $scope.saveApplication();
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.gridOptions = {
            //multiSelect: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            enableRowSelection: true,
            enableFullRowSelection: true,
            selectedItems: $scope.selectedTasks,
            columnDefs: [{ field: 'Sequence', displayName: 'Order', type: 'number', width: "8%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 } },
                         { field: 'Description', displayName: 'Description', width: "62%" },
                         { field: 'PrestoTaskType', displayName: 'Type', width: "16%" },
                         { field: 'FailureCausesAllStop', displayName: 'Stop', width: "12%" }]            
        };

        // Online demo shows setting this after defining the columns. Don't know why.
        $scope.gridOptions.multiSelect = true;

        // ---------------------------------------------------------------------------------------------------

        $scope.gridOptions.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            $scope.gridApi.selection.setModifierKeysToMultiSelect(true); // Allow ctrl-click or shift-click to select multiple rows.
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                // Assign the selectred rows to our tasks variable.
                $scope.selectedTasks = gridApi.selection.getSelectedRows();
            });
        };

        // ---------------------------------------------------------------------------------------------------

        $scope.gridOptions2 = {
            data: 'app.CustomVariableGroups',
            multiSelect: false,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            columnDefs: [{ field: 'Name', displayName: 'Name', width: "98%", resizable: true }]
        };

        // ---------------------------------------------------------------------------------------------------

        $scope.editTask = function() {
            openTask($scope.selectedTasks[0]);
        }

        // ---------------------------------------------------------------------------------------------------

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

                saveApplication();
            }, function () {
                // modal dismissed
            });
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.saveApplication = function () {
            console.log('app: ', $scope.app);
            var config = {
                url: '/PrestoWeb/api/app/saveApplication',
                method: 'POST',
                data: $scope.app
            };

            $scope.loading = 1;
            $http(config)
                .then(function (response) {
                    $scope.app = response.data;
                    $scope.gridOptions.data = response.data.Tasks;
                    $scope.loading = 0;
                    $rootScope.setUserMessage("App saved");
                    $scope.isDirty = false;
                }, function (response) {
                    $scope.loading = 0;
                    console.log(response);
                    showInfoModal.show(response.statusText, response.data);
                });
        }
        
        // ---------------------------------------------------------------------------------------------------

        $http.get('/PrestoWeb/api/app/' + $scope.appId)
            .then(function (response) {
                $scope.app = response.data;
                $scope.loading = 0;
                $scope.gridOptions.data = $scope.app.Tasks;
            },
            function () {
                $scope.loading = 0;
                alert("An error occurred and the app could not be loaded.");
            });
    }           

})();