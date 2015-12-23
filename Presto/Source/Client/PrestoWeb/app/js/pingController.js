﻿(function () {

    'use strict';

    angular.module('myApp.controllers').controller('pingController', pingController);

    // ------------------------------- Ping Controller -------------------------------

    function pingController($scope, $http, $routeParams, pingRequestRepository, pingResponseRepository, uiGridConstants) {
        $scope.loading = 1;
        $scope.pings = null;
        $scope.latestPingRequest;
        $scope.latestPingRequestTime;

        $scope.gridPing= {
            data: 'pings',
            multiSelect: false,
            enableRowHeaderSelection: false, // We don't want to have to click a row header to select a row. We want to just click the row itself.
            // Got the row template from https://github.com/cdwv/ui-grid-draggable-rows:
            rowTemplate: '<div grid="grid" class="ui-grid-draggable-row" draggable="true"><div ng-repeat="(colRenderIndex, col) in colContainer.renderedColumns track by col.colDef.name" class="ui-grid-cell" ng-class="{ \'ui-grid-row-header-cell\': col.isRowHeader, \'custom\': true }" ui-grid-cell></div></div>',
            columnDefs: [{ field: 'ApplicationServer.Name', displayName: 'Server', width: "25%", resizable: true, sort: { direction: uiGridConstants.ASC, priority: 1 } },
                         { field: 'ResponseTime', displayName: 'Response Time', width: "25%", resizable: true },
                         { field: 'Comment', displayName: 'Comment', width: "48%", resizable: true}]
        };

        $scope.refresh = function (forceRefresh) {
            $scope.loading = 1;
            // Since the eventual $http call is async, we have to provide a callback function to use the data retrieved.

            pingRequestRepository.getLatestPingRequest(forceRefresh, function (dataResponse) {
                $scope.latestPingRequest = dataResponse;
                $scope.latestPingRequestTime = moment($scope.latestPingRequest.RequestTime).format('DD-MMM-YYYY HH:mm:ss');

                pingResponseRepository.getResponses(forceRefresh, $scope.latestPingRequest, function (pingResponses) {
                    $scope.pings = pingResponses;
                    $scope.loading = 0;
                });
            });
        };

        $scope.refresh();
    }

})();