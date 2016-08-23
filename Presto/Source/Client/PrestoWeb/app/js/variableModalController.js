(function () {

    'use strict';

    angular.module('myApp.controllers').controller('variableModalController', variableModalController);

    function variableModalController($http, $scope, $uibModalInstance, showInfoModal, variable) {
        $scope.variable = variable;

        // In case the user cancels after making changes:
        if (variable) {
            $scope.initialKey = variable.Key;
            $scope.initialValue = variable.Value;
            $scope.initialValueIsEncrypted = variable.ValueIsEncrypted;
        }

        // -----------------------------------------------------------------------------

        $scope.ok = function () {
            $uibModalInstance.close($scope.variable);
        };

        // -----------------------------------------------------------------------------

        $scope.cancel = function () {
            // Reset the variable to its initial state and close.
            if ($scope.variable) {
                $scope.variable.Key = $scope.initialKey;
                $scope.variable.Value = $scope.initialValue;
                $scope.variable.ValueIsEncrypted = $scope.initialValueIsEncrypted;
            }
            
            $uibModalInstance.dismiss();
        };

        // -----------------------------------------------------------------------------

        $scope.clearValue = function () {
            $scope.variable.Value = '';
            $scope.variable.ValueIsEncrypted = false;
        }

        // -----------------------------------------------------------------------------

        $scope.encryptValue = function () {
            var config = {
                url: '/PrestoWeb/api/variableGroups/encrypt/?valueToEncrypt=' + $scope.variable.Value,
                method: 'GET'
            };

            $scope.loading = 1;
            $http(config)
                .then(function (result) {
                    $scope.variable.Value = result.data;
                    $scope.variable.ValueIsEncrypted = true;
                    $scope.loading = 0;
                }, function (response) {
                    $scope.loading = 0;
                    console.log(response);
                    showInfoModal.show(response.statusText, response.data);
                });
        }
    };
})();