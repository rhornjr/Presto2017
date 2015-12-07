(function () {

    'use strict';

    angular.module('myApp.controllers').controller('globalController', globalController);

    // ------------------------------- Global Controller -------------------------------

    function globalController($scope, $http) {
        // Get the service address so the user can see where he's connected.
        $http.get('/PrestoWeb/api/utility/GetServiceAddress')
            .then(function (result) {
                $scope.serviceAddress = result.data;
            });
    }

})();