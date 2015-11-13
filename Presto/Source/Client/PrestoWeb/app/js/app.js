'use strict';

// To debug this web app, make sure PrestoWeb is the startup project, then hit the Start (Internet Explorer, etc.) button.
// This will start the Web API and the web app. Then navigate to http://localhost/PrestoWeb/app. This allows us to even
// step through the JavaScript code.
// Note: Since this is all deployed to IIS, Visual Studio does not have to be used to run the app. Just browse to the link above.

// Declare app level module which depends on filters, and services
var app = angular.module('myApp', [
  'ngAnimate',
  'ngRoute',
  'myApp.filters',
  'myApp.services',
  'myApp.directives',
  'myApp.controllers',
  'ui.bootstrap',
  'ui.grid',  // http://ui-grid.info/
  'ui.grid.selection',
  'ui.grid.draggable-rows'  // https://github.com/cdwv/ui-grid-draggable-rows
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

            $http.get('/PrestoWeb/api/apps/')
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

            $http.get('/PrestoWeb/api/servers/')
                .then(function (result) {
                    data = result.data;
                    lastRefreshTime = new Date();
                    callbackFunction(data, lastRefreshTime);
                });
        }
    }
}]);

app.factory('logRepository', ['$http', function ($http) {

    var data;
    var lastRefreshTime;

    return {
        getLogs: function (forceRefresh, callbackFunction) {
            if (data && forceRefresh == false) {
                callbackFunction(data, lastRefreshTime);  // If we already have the data, just return it.
                return;
            }

            $http.get('/PrestoWeb/api/log/')
                .then(function (result) {
                    data = result.data;
                    lastRefreshTime = new Date();
                    callbackFunction(data, lastRefreshTime);
                });
        }
    }
}]);

app.filter('escape', function () {
    return function (input) {
        return encodeURIComponent(input);
    }
});

app.run(function ($rootScope, $location) {
    // See http://stackoverflow.com/q/25372095/279516 for why this stuff is here. Basically, it's because RavenDB uses
    // slashes in its IDs, and AngularJS couldn't pass that.
    $rootScope.go = $location.path.bind($location);
    $rootScope.gotoApp = function (appId) {
        $rootScope.go('/app/' + encodeURIComponent(appId));
    };
});

app.config(['$routeProvider', function($routeProvider) {
    $routeProvider.when('/apps', { templateUrl: 'partials/apps.html', controller: 'appsController' });
    $routeProvider.when('/servers', { templateUrl: 'partials/servers.html', controller: 'serversController' });
    $routeProvider.when('/app/:appId?', { templateUrl: 'partials/app.html', controller: 'appController' });
    $routeProvider.when('/server/:serverId?', { templateUrl: 'partials/server.html', controller: 'serverController' });
    $routeProvider.when('/variableGroups', { templateUrl: 'partials/variableGroups.html', controller: 'variableGroupsController' });
    $routeProvider.when('/installs', { templateUrl: 'partials/installs.html', controller: 'installsController' });
    $routeProvider.when('/log', { templateUrl: 'partials/log.html', controller: 'logController' });
    $routeProvider.when('/global', { templateUrl: 'partials/global.html', controller: 'globalController' });
    $routeProvider.otherwise({ redirectTo: '/global' });
}]);
