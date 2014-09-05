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
  .controller('appController', function ($scope, $http, $routeParams) {
      $scope.appId = $routeParams.appId;
      var modifiedAppId = $scope.appId.replace("/", "^^");  // Because we shouldn't send slashes in a web API call.
      $http.get('http://localhost/PrestoWebApi/api/app/' + modifiedAppId)
                .then(function (result) {
                    $scope.app = result.data;
                });
  })
  .controller('installsController', function ($scope, $http) {
      $scope.loading = 1;
      $scope.installs = null;

      $scope.gridOptions = { data: 'installs' };

      $http.get('http://localhost/PrestoWebApi/api/installs/')
            .then(function (result) {
                $scope.installs = result.data;
                $scope.loading = 0;
            });      
  });
