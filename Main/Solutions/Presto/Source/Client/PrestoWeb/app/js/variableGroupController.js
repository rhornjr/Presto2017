(function () {

    'use strict';

    angular.module('myApp.controllers').controller('variableGroupController', variableGroupController);

    // ------------------------------- Variable Group Controller -------------------------------

    function variableGroupController($scope, $rootScope, $http, $routeParams, uiGridConstants, showInfoModal) {
        $scope.group = {};

        // ---------------------------------------------------------------------------------------------------

        $http.get('/PrestoWeb/api/variableGroups/' + $routeParams.groupId)
            .then(function (response) {
                $scope.group = response.data;
                $scope.loading = 0;
            },
            function (response) {
                $scope.loading = 0;
                showInfoModal.show(response.statusText, response.data);
            });
    }
})();