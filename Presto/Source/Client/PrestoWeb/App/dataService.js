define(['model', 'services/logger'],
    function (model, logger) {

    var getApps = function(appsObservable) {

        appsObservable([]);  // reset

        var options = {
            url: '/HotTowel/GetApps',
            type: 'POST',
            dataType: 'json'
        };

        return $.ajax(options)
            .then(onAppsRetrieved)
            .fail(loadingAppsFailed);

        function onAppsRetrieved(data) {
            var apps = [];
            data.sort(sortApps);
            data.forEach(function (app) {
                var knockoutApp = new model.ConvertAppToKnockoutApp(app);
                apps.push(knockoutApp);
            });
            appsObservable(apps);  // Add all apps at once, so we don't get a notification for each push
            logger.log('Loaded apps', apps, true);
        }

        function loadingAppsFailed(jqXhr, textStatus) {
            var message = 'Apps failed to load. ' + textStatus;
            logger.logError(message, jqXhr, true);
        }

        function sortApps(s1, s2) {
            return (s1.name > s2.name) ? 1 : -1;
        }
    };

    var dataService = {
        getApps: getApps
    };

    return dataService;
});