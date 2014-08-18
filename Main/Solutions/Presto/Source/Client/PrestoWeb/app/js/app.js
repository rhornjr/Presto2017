'use strict';

// Declare app level module which depends on filters, and services
var app = angular.module('myApp', [
  'ngAnimate',
  'ngRoute',
  'myApp.filters',
  'myApp.services',
  'myApp.directives',
  'myApp.controllers'
]);

app.factory('appsRepository', ['$http', function ($http) {
    // The factory exists so we only load this data once. If it was in the controller, the Presto service would be called every time
    // we went to the app web page.
    // This is what helped me get this to work: http://stackoverflow.com/a/20369746/279516

    var data;
    var lastRefreshTime;

    return {
        getApps: function (callbackFunction) {
            if (data) {
                callbackFunction(data, lastRefreshTime);
                return;
            }

            $http.get('http://localhost/PrestoWebApi/api/apps/')
                .then(function (result) {
                    data = result.data;
                    lastRefreshTime = new Date();
                    callbackFunction(data, lastRefreshTime);
                });
        }
    }
}]);

app.config(['$routeProvider', function($routeProvider) {
    $routeProvider.when('/apps', { templateUrl: 'partials/apps.html', controller: 'appsController' });
    $routeProvider.when('/servers', { templateUrl: 'partials/servers.html', controller: 'serversController' });
    $routeProvider.otherwise({ redirectTo: '/apps' });
}]);
