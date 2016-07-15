﻿(function () {

    'use strict';

    angular.module('myApp.controllers').controller('globalController', globalController);

    // ------------------------------- Global Controller -------------------------------

    function globalController($scope, $http, showInfoModal) {
        // Get the service address so the user can see where he's connected.
        $http.get('/PrestoWeb/api/utility/getServiceAddress')
            .then(function (result) {
                $scope.serviceAddress = result.data;
            }, function (response) {
                console.log(response);
                showInfoModal.show(response.statusText, response.data);
            });
    }

})();