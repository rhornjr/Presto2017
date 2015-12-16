﻿(function () {

    'use strict';

    angular.module('myApp.controllers').controller('logController', logController);

    // ------------------------------- Log Controller -------------------------------

    function logController($scope, logRepository) {
        $scope.loading = 1;
        $scope.logMessages = null;

        $scope.gridOptions = {
            data: 'logMessages',
            multiSelect: false,
            columnDefs: [{ field: 'MessageCreatedTime', displayName: 'Time', width: "20%", resizable: true },
                         { field: 'Message', displayName: 'Message', width: "58%", resizable: true },
                         { field: 'UserName', displayName: 'User', width: "20%", resizable: true }]
        };

        $scope.refresh = function (forceRefresh) {
            // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.          
            logRepository.getLogs(forceRefresh, function (dataResponse, lastRefreshTime) {
                $scope.logMessages = dataResponse;
                $scope.lastRefreshTime = lastRefreshTime;
                $scope.loading = 0;
            });
        };

        $scope.refresh(false);
    }

})();