(function () {

    'use strict';

    angular.module('myApp.controllers').controller('globalController', globalController);

    // ------------------------------- Global Controller -------------------------------

    function globalController($rootScope, $scope, $http, globalState, showInfoModal) {
        $scope.state = globalState;

        var getServiceAddress = function () {
            // Get the service address so the user can see where he's connected.
            $http.get('/PrestoWeb/api/utility/getServiceAddress')
                .then(function (result) {
                    $scope.state.serviceAddress = result.data;
                }, function (response) {
                    console.log(response);
                    showInfoModal.show(response.statusText, response.data);
                });
        }

        var getGlobalSettings = function () {
            $scope.loading = 1;
            $http.get('/PrestoWeb/api/global')
                .then(function (result) {
                    $scope.loading = 0;
                    $scope.state.globalSetting = result.data;
                }, function (response) {
                    $scope.loading = 0;
                    console.log(response);
                    showInfoModal.show(response.statusText, response.data);
                });
        }

        if (!$scope.state.retrieved) {
            getServiceAddress();
            getGlobalSettings();
            $scope.state.retrieved = true;
        }

        $scope.saveGlobalSetting = function () {
            var config = {
                url: '/PrestoWeb/api/global/save',
                method: 'POST',
                data: $scope.state.globalSetting
            };

            $scope.loading = 1;
            $http(config)
                .then(function (response) {
                    $scope.state.globalSetting = response.data;
                    $scope.loading = 0;
                    $rootScope.setUserMessage("Global settings saved");
                    $scope.globalForm.$dirty = false;
                }, function (response) {
                    $scope.loading = 0;
                    console.log(response);
                    showInfoModal.show(response.statusText, response.data);
                });
        }

        $scope.setIsDirty = function () {
            $scope.globalForm.$dirty = true;
        }
    }

})();