define(['services/logger'], function (logger) {
    var vm = {
        activate: activate,
        title: 'Log'
    };

    return vm;

    //#region Internal Methods
    function activate() {
        logger.log('Log View Activated', null, 'log', true);
        return true;
    }
    //#endregion
});