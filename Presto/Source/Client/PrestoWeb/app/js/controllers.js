'use strict';

/* Controllers */

angular.module('myApp.controllers', []).
    controller('appsController', function ($scope, appsRepository) {

        $scope.refresh = function (forceRefresh) {
            // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.
            $scope.loading = 1;
            $scope.apps = null;
            appsRepository.getApps(forceRefresh, function (dataResponse, lastRefreshTime) {
                $scope.apps = dataResponse;
                $scope.lastRefreshTime = lastRefreshTime;
                $scope.loading = 0;
            });
        };

        // ----------------------------------------------------------------------------------------

        $scope.refresh(false);
  })
  .controller('serversController', ['$scope', '$http', function ($scope, $http) {
      // Commented just for testing so we don't have to wait for the servers to load.
      //$http.get('http://localhost/PrestoWebApi/api/servers/')
      //  .then(function (result) {
      //      $scope.servers = result.data;
      //  });
  }]);
