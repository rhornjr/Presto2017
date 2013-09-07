define(['services/logger'], function (logger) {
    var vm = {
        activate: activate,
        title: 'Applications'
    };

    return vm;

    //#region Internal Methods
    function activate() {
        logger.log('Apps View Activated', null, 'apps', true);
        return true;
    }
    //#endregion
});