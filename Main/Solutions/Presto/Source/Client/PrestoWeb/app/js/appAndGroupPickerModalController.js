(function () {

    'use strict';

    angular.module('myApp.controllers').controller('appAndGroupPickerModalController', appAndGroupPickerModalController);

    function appAndGroupPickerModalController($scope, $uibModalInstance, $uibModal, appWithGroups) {
        $scope.appWithGroups = {};
        if (appWithGroups) {
            $scope.appWithGroups = {
                app: appWithGroups.Application,
                groups: appWithGroups.CustomVariableGroups,
                groupNames: appWithGroups.CustomVariableGroupNames,
                enabled: appWithGroups.Enabled
            };
        }

        $scope.pickApp = function () {
            var modalInstance = $uibModal.open({
                templateUrl: 'partials/appPicker.html',
                controller: 'appPickerModalController',
                size: 'sm',
                windowClass: 'app-modal-window'
            });

            modalInstance.result.then(function (app) {
                $scope.appWithGroups.app = app;
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
                $scope.appWithGroups.groups = overrides;
                // Show the list of names to the user.
                $scope.appWithGroups.groupNames = '';
                for (var i = 0; i < overrides.length; i++) {
                    $scope.appWithGroups.groupNames += overrides[i].Name + " | ";
                }
            }, function () {
                // modal dismissed
            });
        }

        // ---------------------------------------------------------------------------------------------------

        $scope.ok = function () {
            $uibModalInstance.close($scope.appWithGroups);
        };

        // ---------------------------------------------------------------------------------------------------

        $scope.cancel = function () {
            $uibModalInstance.dismiss();
        };
    };
})();