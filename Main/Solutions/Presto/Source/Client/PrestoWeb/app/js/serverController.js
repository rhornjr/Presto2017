(function () {

    'use strict';

    angular.module('myApp.controllers').controller('serverController', serverController);

    // ------------------------------- Server Controller -------------------------------

    function serverController($scope, $rootScope, $http, $routeParams, $window, uiGridConstants, $uibModal, showConfirmationModal, showInfoModal) {
        $scope.server = {};
        $scope.serverId = $routeParams.serverId;
        $scope.selectedAppsWithGroup = [];
        $scope.selectedGroups = [];
        $scope.environments = [];

        // ---------------------------------------------------------------------------------------------------

        $scope.gridAppsWithGroup = {
            data: 'server.ApplicationsWithOverrideGroup',
            multiSelect: false,
            enableColumnResizing: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            enableFiltering: false,
            columnDefs: [{ field: 'Application.Name', displayName: 'App', width: "26%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 } },
                         { field: 'Application.Version', displayName: 'Version', width: "12%" },
                         { field: 'CustomVariableGroupNames', displayName: 'Overrides', width: "48%" },
                         { field: 'Enabled', displayName: 'Enabled', width: "12%" }]
        };

        // ---------------------------------------------------------------------------------------------------

        $scope.gridGroups = {
            data: 'server.CustomVariableGroups',
            multiSelect: false,
            enableColumnResizing: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            columnDefs: [{ field: 'Name', displayName: 'Name', width: "98%", resizable: true }]
        };

        // ---------------------------------------------------------------------------------------------------

        $scope.loading = 1;
        $http.get('/PrestoWeb/api/server/getEnvironments')
            .then(function (result) {
                $scope.environments = result.data;
                $scope.loading = 0;
            },
            function (result) {
                $scope.loading = 0;
                showInfoModal.show(response.statusText, response.data);
            });

        // ---------------------------------------------------------------------------------------------------

        if ($scope.serverId) {
            $scope.loading = 1;
            $http.get('/PrestoWeb/api/server/' + $scope.serverId)
                .then(function (response) {
                    $scope.server = response.data;
                    setCustomVariableGroupNames();
                    $scope.loading = 0;
                },
                function (response) {
                    $scope.loading = 0;
                    showInfoModal.show(response.statusText, response.data);
                });
        }

        // ---------------------------------------------------------------------------------------------------

        var setCustomVariableGroupNames = function () {
            // Concatenate the group names for display
            var appsWithGroupsCount = $scope.server.ApplicationsWithOverrideGroup.length;
            for (var i = 0; i < appsWithGroupsCount; i++) {
                var appWithGroups = $scope.server.ApplicationsWithOverrideGroup[i];
                if (!appWithGroups.CustomVariableGroups) { continue; }
                appWithGroups.CustomVariableGroupNames = '';
                for (var j = 0; j < appWithGroups.CustomVariableGroups.length; j++) {
                    appWithGroups.CustomVariableGroupNames += appWithGroups.CustomVariableGroups[j].Name + " | ";
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.setIsDirty = function () {
            // For some reason, the checkbox doesn't cause $dirty to update after the page is reloaded.
            // So, as a hack, just do it here.
            $scope.serverForm.$dirty = true;
        }

        // ---------------------------------------------------------------------------------------------------

        // Act on the row selection changing.
        $scope.gridAppsWithGroup.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                console.log(row);  // This is a nice option. It allowed my to browse the object and discover that I wanted the entity property.
                $scope.selectedAppsWithGroup.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.selectedAppsWithGroup.push(row.entity);
            });
        };

        // ---------------------------------------------------------------------------------------------------

        // Act on the row selection changing.
        $scope.gridGroups.onRegisterApi = function (gridGroupsApi) {
            $scope.gridGroupsApi = gridGroupsApi;
            gridGroupsApi.selection.on.rowSelectionChanged($scope, function (row) {
                console.log(row);  // This is a nice option. It allowed my to browse the object and discover that I wanted the entity property.
                $scope.selectedGroups.length = 0; // Truncate/clear the array. Yes, this is how it's done.
                $scope.selectedGroups.push(row.entity);
            });
        };

        // ---------------------------------------------------------------------------------------------------

        $scope.addAppAndGroup = function () {
            var modalInstance = $uibModal.open({
                templateUrl: 'partials/appAndGroupPicker.html',
                controller: 'appAndGroupPickerModalController',
                size: 'lg',
                resolve: {
                    appWithGroups: function () {
                        return null;
                    }
                }
            });

            modalInstance.result.then(function (appAndGroups) {
                var newAppWithGroups = {
                    Application: appAndGroups.app,
                    CustomVariableGroups: appAndGroups.groups,
                    CustomVariableGroupNames: appAndGroups.groupNames,
                    Enabled: appAndGroups.enabled
                };
                $scope.server.ApplicationsWithOverrideGroup.push(newAppWithGroups);
                $scope.saveServer();
            }, function () {
                // modal dismissed
            });
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.editAppAndGroup = function () {
            // Get the index of the selected appWithGroup.
            var indexOfGroupBeingEdited = $scope.server.ApplicationsWithOverrideGroup.indexOf($scope.selectedAppsWithGroup[0]);

            var modalInstance = $uibModal.open({
                templateUrl: 'partials/appAndGroupPicker.html',
                controller: 'appAndGroupPickerModalController',
                size: 'lg',
                resolve: {
                    appWithGroups: function () {
                        return $scope.selectedAppsWithGroup[0];
                    }
                }
            });

            modalInstance.result.then(function (appAndGroups) {
                var newAppWithGroups = {
                    Application: appAndGroups.app,
                    CustomVariableGroups: appAndGroups.groups,
                    CustomVariableGroupNames: appAndGroups.groupNames,
                    Enabled: appAndGroups.enabled
                };
                $scope.server.ApplicationsWithOverrideGroup[indexOfGroupBeingEdited] = newAppWithGroups;
                $scope.saveServer();
            }, function () {
                // modal dismissed
            });
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.removeAppAndGroup = function () {
            showConfirmationModal.show('Delete selected app with overrides?', removeAppAndGroup);
        }

        // ---------------------------------------------------------------------------------------------------

        var removeAppAndGroup = function (confirmed) {
            if (!confirmed) { return; }

            // Get the index of the selected appWithGroup.
            var indexOfGroupBeingDeleted = $scope.server.ApplicationsWithOverrideGroup.indexOf($scope.selectedAppsWithGroup[0]);

            $scope.server.ApplicationsWithOverrideGroup.splice(indexOfGroupBeingDeleted, 1);
            $scope.saveServer();
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.addGroup = function () {
            var modalInstance = $uibModal.open({
                templateUrl: 'partials/variableGroupsPicker.html',
                controller: 'groupsPickerModalController',
                size: 'sm',
                windowClass: 'app-modal-window'
            });

            modalInstance.result.then(function (overrides) {
                for (var i = 0; i < overrides.length; i++) {
                    $scope.server.CustomVariableGroups.push(overrides[i]);
                }
                $scope.saveServer();
            }, function () {
                // modal dismissed
            });
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.deleteGroups = function () {
            showConfirmationModal.show('Delete selected groups?', deleteGroups);
        }

        var deleteGroups = function (confirmed) {
            if (!confirmed) { return; }

            // Remove the selected items from the main list.
            $scope.server.CustomVariableGroups = $scope.server.CustomVariableGroups.filter(function (element) {
                return $scope.selectedGroups.indexOf(element) < 0;
            });

            $scope.saveServer();
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.saveServer = function () {
            var config = {
                url: '/PrestoWeb/api/server/save',
                method: 'POST',
                data: $scope.server
            };

            $scope.loading = 1;
            $http(config)
                .then(function (response) {
                    $scope.server = response.data;
                    setCustomVariableGroupNames();
                    $rootScope.setUserMessage("Server saved.");
                    $scope.selectedAppsWithGroup = [];
                    $scope.loading = 0;
                }, function (response) {
                    $scope.loading = 0;
                    $rootScope.setUserMessage("Save failed");
                    $scope.selectedAppsWithGroup = [];
                    showInfoModal.show(response.statusText, response.data);
                });
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.install = function () {
            showConfirmationModal.show('Install ' + $scope.selectedAppsWithGroup[0].Application.Name + '?', install);
        }

        var install = function (confirmed) {
            if (!confirmed) { return; }

            var entityContainer = {
                server: $scope.server,
                appWithGroup: $scope.selectedAppsWithGroup[0]
            }

            var config = {
                url: '/PrestoWeb/api/server/installapp',
                method: 'POST',
                data: entityContainer
            };

            $http(config)
                .then(function (response) {
                    $rootScope.setUserMessage("Install request sent successfully");
                    showInfoModal.show("Install Request Sent", "Install request sent successfully for " + $scope.selectedAppsWithGroup[0].Application.Name, 3000);
                }, function (response) {
                    $rootScope.setUserMessage("Install request failed");
                    showInfoModal.show(response.statusText, response.data);
                });
        };

        // ---------------------------------------------------------------------------------------------------

        $scope.backToList = function () {
            $window.location.href = '/PrestoWeb/app/#/servers/1';
        }
    }

})();