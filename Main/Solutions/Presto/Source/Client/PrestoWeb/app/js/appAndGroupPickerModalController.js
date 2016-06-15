(function () {

    'use strict';

    angular.module('myApp.controllers').controller('appAndGroupPickerModalController', appAndGroupPickerModalController);

    function appAndGroupPickerModalController($scope, $uibModalInstance, $uibModal) {
        $scope.pickApp = function () {
            var modalInstance = $uibModal.open({
                templateUrl: 'partials/appPicker.html',
                controller: 'appPickerModalController',
                size: 'sm',
                windowClass: 'app-modal-window'
            });

            modalInstance.result.then(function (app) {
                console.log("App picked", app);
                $scope.selectedApp = app;
            }, function () {
                // modal dismissed
            });
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.pickOverrides = function () {
            var modalInstance = $uibModal.open({
                templateUrl: 'partials/variableGroupsPicker.html',
                controller: 'groupsPickerModalController',
                size: 'sm',
                windowClass: 'app-modal-window'
            });

            modalInstance.result.then(function (overrides) {
                console.log("Group(s) picked", overrides);
                $scope.selectedOverrides = overrides;
                // Show the list of names to the user.
                $scope.selectedOverridesNames = '';
                for (var i = 0; i < overrides.length; i++) {
                    $scope.selectedOverridesNames += overrides[i].Name + " | ";
                }
            }, function () {
                // modal dismissed
            });
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.ok = function () {
            var newAppWithGroups = {
                app: $scope.selectedApp,
                groups: $scope.selectedOverrides,
                groupNames: $scope.selectedOverridesNames,
                enabled: $scope.enabled
            };
            $uibModalInstance.close(newAppWithGroups);
        };

        // ---------------------------------------------------------------------------------------------------

        $scope.cancel = function () {
            $uibModalInstance.dismiss();
        };
    };
})();