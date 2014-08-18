'use strict';

// To debug this web app, make sure PrestoWeb is the startup project, then hit the Start (Internet Explorer, etc.) button.
// This will start the Web API and the web app. Then navigate to http://localhost/PrestoWebApi/app. This allows us to even
// step through the JavaScript code.
// Note: Since this is all deployed to IIS, Visual Studio does not have to be used to run the app. Just browse to the link above.

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
        getApps: function (forceRefresh, callbackFunction) {
            if (data && forceRefresh == false) {
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

app.factory('serversRepository', ['$http', function ($http) {

    var data;
    var lastRefreshTime;

    return {
        getServers: function (forceRefresh, callbackFunction) {
            if (data && forceRefresh == false) {
                callbackFunction(data, lastRefreshTime);
                return;
            }

            $http.get('http://localhost/PrestoWebApi/api/servers/')
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
