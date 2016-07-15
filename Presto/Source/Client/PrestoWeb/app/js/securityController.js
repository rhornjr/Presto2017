(function () {

    'use strict';

    angular.module('myApp.controllers').controller('securityController', securityController);

    // ------------------------------- Global Controller -------------------------------

    function securityController($scope, $http, showInfoModal) {
        // Get the service address so the user can see where he's connected.
        $http.get('/PrestoWeb/api/utility/getAuthorizationSettings')
            .then(function (result) {
                $scope.authorizationSetting = result.data;
            }, function (response) {
                console.log(response);
                showInfoModal.show(response.statusText, response.data);
            });
    }

})();