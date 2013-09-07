define(['services/logger', 'dataService'], function (logger, dataService) {

    var apps = ko.observableArray();
    var initialized = false;

    var vm = {
        activate: activate,
        title: 'Applications',
        apps: apps,
        refresh: refresh
    };

    return vm;

    //#region Internal Methods

    function activate() {
        if (initialized) { return; }

        initialized = true;
        logger.log('Apps View Activated', null, 'apps', true);
        return refresh();
        //return true;
    }

    function refresh() {
        return dataService.getApps(apps);
    }

    //#endregion
});