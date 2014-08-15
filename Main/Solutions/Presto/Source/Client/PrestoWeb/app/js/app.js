'use strict';

// Declare app level module which depends on filters, and services
angular.module('myApp', [
  'ngAnimate',
  'ngRoute',
  'myApp.filters',
  'myApp.services',
  'myApp.directives',
  'myApp.controllers'
]).
factory('appsRepository', ['$http', function ($http) {
    // The factory exists so we only load this data once. If it was in the controller, the service would be called every time
    // we went to the app web page.

    var data;

    if (data != null) { return data; }

    return {
        getApps: function (callbackFunction) {
            // This is what helped me get this to work: http://stackoverflow.com/a/20369746/279516
            $http.get('http://localhost/PrestoWebApi/api/apps/')
                .then(function (result) {
                    data = result.data;
                    callbackFunction(result.data);
                });
        }
    }    
}])
.
config(['$routeProvider', function($routeProvider) {
    $routeProvider.when('/apps', { templateUrl: 'partials/apps.html', controller: 'appsController' });
    $routeProvider.when('/servers', { templateUrl: 'partials/servers.html', controller: 'serversController' });
    $routeProvider.otherwise({ redirectTo: '/apps' });
}]);
