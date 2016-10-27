(function () {

    'use strict';

    angular.module('myApp.controllers').controller('pingController', pingController);

    // ------------------------------- Ping Controller -------------------------------

    function pingController($rootScope, $scope, $http, $routeParams, pingRequestRepository, pingResponseRepository, uiGridConstants, showInfoModal) {
        $scope.loading = 1;
        $scope.pings = null;
        $scope.latestPingRequest;
        $scope.latestPingRequestTime;
        var timerIntervalObject = null;
        var pingRefreshIntervalInMilliseconds = 10000;
        var numberOfMinutesToContinuouslyRefresh = 2;

        $scope.gridPing= {
            data: 'pings',
            multiSelect: false,
            enableColumnResizing: true,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            // Got the row template from https://github.com/cdwv/ui-grid-draggable-rows:
            rowTemplate: '<div grid="grid" class="ui-grid-draggable-row" draggable="true"><div ng-repeat="(colRenderIndex, col) in colContainer.renderedColumns track by col.colDef.name" class="ui-grid-cell" ng-class="{ \'ui-grid-row-header-cell\': col.isRowHeader, \'custom\': true }" ui-grid-cell></div></div>',
            columnDefs: [{ field: 'ApplicationServer.Name', displayName: 'Server', width: "25%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 } },
                         { field: 'ResponseTime', displayName: 'Response Time', width: "25%", resizable: true },
                         { field: 'Comment', displayName: 'Comment', width: "50%", resizable: true}]
        };

        $scope.refresh = function (forceRefresh) {
            $scope.loading = 1;
            // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.

            pingRequestRepository.getLatestPingRequest(forceRefresh, function (dataResponse) {
                setPingInfo(dataResponse);
                getPingResponses(forceRefresh);
            });
        };

        $scope.refresh();

        // -----------------------------------------------------------------------------------

        function setPingInfo(pingRequest) {
            $scope.latestPingRequest = pingRequest;
            $scope.latestPingRequestTime = moment($scope.latestPingRequest.RequestTime).format('DD-MMM-YYYY HH:mm:ss');
        }

        // -----------------------------------------------------------------------------------

        function getPingResponses(forceRefresh) {
            $scope.loading = 1;
            pingResponseRepository.getResponses(forceRefresh, $scope.latestPingRequest, function (pingResponses) {
                $scope.pings = pingResponses;
                $scope.loading = 0;
            });
        }

        // -----------------------------------------------------------------------------------

        $scope.sendPingRequest = function () {
            var config = {
                url: '/PrestoWeb/api/ping/sendRequest/',
                method: 'POST'
            };

            $http(config)
                .then(function (response) {
                    $rootScope.setUserMessage("Starting ping refresh every " + pingRefreshIntervalInMilliseconds +
                        " milliseconds for " + numberOfMinutesToContinuouslyRefresh + " minute(s).");
                    showInfoModal.show("Ping Request Sent", "Ping request sent successfully", 3000);
                    setPingInfo(response.data);
                    startTimer(); // Now that we have a new ping, start the timer to get ping responses from servers.
                }, function (response) {
                    showInfoModal.show(response.statusText, response.data);
            });
        }

        // -----------------------------------------------------------------------------------

        function startTimer() {            
            timerIntervalObject = window.setInterval(myTimer, pingRefreshIntervalInMilliseconds);
        }
        
        // -----------------------------------------------------------------------------------

        function myTimer() {
            // Only refresh if we're not already loading from the previous call.
            if ($scope.loading == 1) {
                return;
            }

            // If we've hit our threshold for how long we want to refresh, stop the timer.
            var startTime = moment($scope.latestPingRequest.RequestTime);
            var endTime = moment(Date.now());
            var duration = moment.duration(endTime.diff(startTime));
            var minutes = duration.asMinutes();
            if (minutes >= numberOfMinutesToContinuouslyRefresh) {
                window.clearInterval(timerIntervalObject);
                $rootScope.setUserMessage("Done refreshing pings");
                return;
            }

            getPingResponses(true);
        }
    }

})();