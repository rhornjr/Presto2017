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
  .controller('serversController', function ($scope, serversRepository) {

      $scope.refresh = function (forceRefresh) {
          // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.
          $scope.loading = 1;
          $scope.servers = null;
          serversRepository.getServers(forceRefresh, function (dataResponse, lastRefreshTime) {
              $scope.servers = dataResponse;
              $scope.lastRefreshTime = lastRefreshTime;
              $scope.loading = 0;
          });
      };

      // ----------------------------------------------------------------------------------------

      $scope.refresh(false);
  })
  .controller('appController', function ($scope, $routeParams) {
      $scope.appId = $routeParams.appId;
});
