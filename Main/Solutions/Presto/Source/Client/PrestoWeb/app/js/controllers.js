'use strict';

/* Controllers */

angular.module('myApp.controllers', []).
  controller('appsController', ['$scope', '$http', function ($scope, $http) {
      $http.get('http://fs-6103/PrestoWebApi/api/apps/')
        .then(function (result) {
            $scope.apps = result.data;
        });
  }])
  .controller('serversController', ['$scope', '$http', function ($scope, $http) {
  }]);
