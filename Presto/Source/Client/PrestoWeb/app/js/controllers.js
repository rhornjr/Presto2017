'use strict';

/* Controllers */

angular.module('myApp.controllers', []).
    controller('appsController', function ($scope, appsRepository) {
        // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.
        $scope.loading = 1;
        appsRepository.getApps(function (dataResponse) {
            $scope.apps = dataResponse;
            $scope.lastRefreshTime = new Date();
            $scope.loading = 0;
        });        
  })
  .controller('serversController', ['$scope', '$http', function ($scope, $http) {
      // Commented just for testing so we don't have to wait for the servers to load.
      //$http.get('http://localhost/PrestoWebApi/api/servers/')
      //  .then(function (result) {
      //      $scope.servers = result.data;
      //  });
  }]);
