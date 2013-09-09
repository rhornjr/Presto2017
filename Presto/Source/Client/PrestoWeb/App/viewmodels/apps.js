define(['services/logger', 'dataService', 'durandal/plugins/router'],
    function (logger, dataService, router) {

    var apps = ko.observableArray();
    var initialized = false;

    var vm = {
        activate: activate,
        canActivate: canActivate,
        title: 'Applications',
        apps: apps,
        refresh: refresh
    };

    return vm;

    //#region Internal Methods

    function activate() {
        initialized = true;
        logger.log('Apps View Activated', null, 'apps', true);
        return refresh();
    }

    // Got the idea to use canActivate from http://stackoverflow.com/a/18679833/279516.
    function canActivate() {
        // if (initialized) { router.navigateTo("#/ping"); return false; }  // testing

        return true;
    }

    function refresh() {
        return dataService.getApps(apps);
    }

    //#endregion
});