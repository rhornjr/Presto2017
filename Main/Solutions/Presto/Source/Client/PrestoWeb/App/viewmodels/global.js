define(['services/logger'], function (logger) {
    var vm = {
        activate: activate,
        title: 'Global'
    };

    return vm;

    //#region Internal Methods
    function activate() {
        logger.log('Global View Activated', null, 'global', true);
        return true;
    }
    //#endregion
});