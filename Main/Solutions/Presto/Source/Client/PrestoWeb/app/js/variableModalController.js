(function () {

    'use strict';

    angular.module('myApp.controllers').controller('variableModalController', variableModalController);

    function variableModalController($scope, $uibModalInstance, variable) {
        $scope.variable = variable;

        $scope.ok = function () {
            $uibModalInstance.close($scope.variable);
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss();
        };
    };
})();