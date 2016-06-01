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

angular.module('myApp.controllers', []);

// When a file is read, set the contents property so it can be used in the controller.
// http://stackoverflow.com/a/27303454/279516
// http://jsfiddle.net/200eoamf/1/
app.directive('onReadFile', function ($parse) {
    return {
        restrict: 'A',
        scope: false,
        link: function(scope, element, attrs) {
            element.bind('change', function(e) {
                
                var onFileReadFn = $parse(attrs.onReadFile);
                var reader = new FileReader();
                
                reader.onload = function() {
                    var fileContents = reader.result;
                    // invoke parsed function on scope special syntax for passing in data to named parameters.
                    // in the parsed function we are providing a value for the property 'contents'.
                    // in the scope we pass in to the function.
                    scope.$apply(function() {
                        onFileReadFn(scope, {
                            'contents' : fileContents
                        });
                    });
                };
                reader.readAsText(element[0].files[0]);
            });
        }
    };
})

// --------------------------------------------------------------------------------------

app.run(function ($rootScope) {
        $rootScope.$on('$routeChangeSuccess', function (ev, data) {

            // When the route changes, set activeController variable to the actual active controller.
            // This allows the NavBar stylings to be correct on index.html.
            if (data.$$route && data.$$route.controller) {
                $rootScope.activeController = data.$$route.controller;
            }
        });
    });

// ------------------------------- Info Modal Controller -------------------------------

angular.module('myApp.controllers').controller('infoController', function ($scope, $uibModalInstance, title, message) {
    $scope.title = title;
    $scope.message = message
    $scope.ok = function () {
        $uibModalInstance.close();
    };
});

// --------------------------------------------------------------------------------------

app.factory('showInfoModal', ['$http', '$rootScope', '$uibModal', function ($http, $rootScope, $uibModal) {
    return {
        show: function (title, message) {
            var modalInstance = $uibModal.open({
                templateUrl: 'partials/infoModal.html',
                controller: 'infoController',
                size: 'sm',
                windowClass: 'app-modal-window',
                resolve: {
                    title: function () {
                        return title;
                    },
                    message: function () {
                        return message;
                    }
                }
            });
        }
    }
}]);

// --------------------------------------------------------------------------------------

app.factory('appsRepository', ['$http', '$rootScope', function ($http, $rootScope) {
    // The factory exists so we only load this data once. If it was in the controller, the Presto service would be called every time
    // we went to the app web page.
    // This is what helped me get this to work: http://stackoverflow.com/a/20369746/279516

    var data;

    return {
        getApps: function (forceRefresh, callbackFunction) {
            if (data && !forceRefresh) {
                callbackFunction(data);
                return;
            }

            $http.get('/PrestoWeb/api/apps/')
                .then(function (result) {
                    data = result.data;
                    $rootScope.setUserMessage("Application list refreshed");
                    callbackFunction(data);
                }, function (response) {
                    console.log(response);
                    if (response.status == 403) {
                        $rootScope.setUserMessage("Unauthorized");
                        callbackFunction(null);
                    }
                });
        }
    }
}]);

app.factory('serversRepository', ['$http', '$rootScope', function ($http, $rootScope) {

    var data;

    return {
        getServers: function (forceRefresh, callbackFunction) {
            if (data && !forceRefresh) {
                callbackFunction(data);
                return;
            }

            $http.get('/PrestoWeb/api/servers/')
                .then(function (result) {
                    data = result.data;
                    $rootScope.setUserMessage("Server list refreshed");
                    callbackFunction(data);
                });
        }
    }
}]);

app.factory('logRepository', ['$http', '$rootScope', function ($http, $rootScope) {

    var data;

    return {
        getLogs: function (forceRefresh, callbackFunction) {
            if (data && !forceRefresh) {
                callbackFunction(data);  // If we already have the data, just return it.
                return;
            }

            $http.get('/PrestoWeb/api/log/')
                .then(function (result) {
                    data = result.data;
                    $rootScope.setUserMessage("Log list refreshed");
                    callbackFunction(data);
                });
        }
    }
}]);

app.factory('variableGroupsRepository', ['$http', '$rootScope', function ($http, $rootScope) {
    var data;

    return {
        getVariableGroups: function (forceRefresh, callbackFunction) {
            if (data && !forceRefresh) {
                callbackFunction(data);
                return;
            }

            $http.get('/PrestoWeb/api/variableGroups/')
                .then(function (result) {
                    data = result.data;
                    $rootScope.setUserMessage("Variable group list refreshed");
                    callbackFunction(data);
                });
        }
    }
}]);

app.factory('resolveRepository', ['$http', '$rootScope', function ($http, $rootScope) {
    var dataContainer;

    return {
        getResolvedVariables: function (forceRefresh, app, server, overrides, callbackFunction) {
            if (dataContainer && !forceRefresh) {
                // We already have data, so let's return it so the page doesn't lose it's data when returning to it.
                callbackFunction(dataContainer);
                return;
            }

            if (!app || !server) // Ignore if no app/server selected.
            {
                callbackFunction(null);
                return;
            }

            // Create *one* object because we can't pass multiple parameters to a web API method.
            var appAndServerAndOverrides = {
                application: app,
                server: server,
                overrides: overrides
            }

            var config = {
                url: '/PrestoWeb/api/resolve/resolveVariables',
                method: 'POST',
                data: appAndServerAndOverrides
            };

            $http(config)
                .then(function (response) {
                    $rootScope.setUserMessage("Variables resolved");
                    // Store the resolved variables along with other data so the page doesn't lose its info when switching tabs.
                    dataContainer = {};
                    dataContainer.data = response.data;
                    dataContainer.app = app;
                    dataContainer.server = server;
                    dataContainer.overrides = overrides;
                    callbackFunction(dataContainer);
                }, function (response) {
                    console.log(response);
                });
        }
    }
}]);

app.factory('installsRepository', ['$http', '$rootScope', function ($http, $rootScope) {
    var data;

    return {
        getInstalls: function (forceRefresh, callbackFunction) {
            if (data && !forceRefresh) {
                callbackFunction(data);
                return;
            }

            $http.get('/PrestoWeb/api/installs/')
                .then(function (result) {
                    data = result.data;
                    $rootScope.setUserMessage("Installs list refreshed");
                    callbackFunction(data);
                });
        }
    }
}]);

app.factory('pendingInstallsRepository', ['$http', '$rootScope', function ($http, $rootScope) {
    var data;

    return {
        getPending: function (forceRefresh, callbackFunction) {
            if (data && !forceRefresh) {
                callbackFunction(data);
                return;
                }

            $http.get('/PrestoWeb/api/pendingInstalls/')
                    .then(function (result) {
                        data = result.data;
                        $rootScope.setUserMessage("Pending installs list refreshed");
                    callbackFunction(data);
                });
        }
    }
}]);

app.factory('pingResponseRepository', ['$http', '$rootScope', function ($http, $rootScope, latestPingRequest) {
    var data;

    return {
        getResponses: function (forceRefresh, latestPingRequest, callbackFunction) {
            if (data && !forceRefresh) {
                callbackFunction(data);
                return;
            }

            var config = {
                url: '/PrestoWeb/api/ping/responses/',
                method: 'POST',
                data: latestPingRequest
            };

            $http(config)
                .then(function (response) {
                    data = response.data;
                    $rootScope.setUserMessage("Ping list refreshed");
                    callbackFunction(data);
                }, function (response) {
                    alert(response);
            });
        }
    }
}]);

app.factory('pingRequestRepository', ['$http', '$rootScope', function ($http, $rootScope) {
    var data;

    return {
        getLatestPingRequest: function (forceRefresh, callbackFunction) {
            if (data && !forceRefresh) {
                callbackFunction(data);
                return;
            }

            $http.get('/PrestoWeb/api/ping/latestRequest/')
                .then(function (result) {
                    data = result.data;
                    $rootScope.setUserMessage("Retrieved latest ping request");
                    callbackFunction(data);
                },
                function (result) {
                    alert(result);
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

    $rootScope.setUserMessage = function (userMessage) {
        $rootScope.userMessage = userMessage + " - " + moment().format('DD-MMM HH:mm:ss');
    }

    $rootScope.setUserMessage('Presto started');
});

app.config(['$routeProvider', function($routeProvider) {
    $routeProvider.when('/apps', { templateUrl: 'partials/apps.html', controller: 'appsController' });
    $routeProvider.when('/servers', { templateUrl: 'partials/servers.html', controller: 'serversController' });
    $routeProvider.when('/app/:appId?', { templateUrl: 'partials/app.html', controller: 'appController' });
    $routeProvider.when('/server/:serverId?', { templateUrl: 'partials/server.html', controller: 'serverController' });
    $routeProvider.when('/variableGroups', { templateUrl: 'partials/variableGroups.html', controller: 'variableGroupsController' });
    $routeProvider.when('/resolve', { templateUrl: 'partials/resolve.html', controller: 'resolveController' });
    $routeProvider.when('/installs', { templateUrl: 'partials/installs.html', controller: 'installsController' });
    $routeProvider.when('/log', { templateUrl: 'partials/log.html', controller: 'logController' });
    $routeProvider.when('/ping', { templateUrl: 'partials/ping.html', controller: 'pingController' });
    $routeProvider.when('/global', { templateUrl: 'partials/global.html', controller: 'globalController' });
    $routeProvider.otherwise({ redirectTo: '/global' });
}]);
