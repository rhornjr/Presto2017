define(['services/logger'], function (logger) {
    var vm = {
        activate: activate,
        title: 'Servers'
    };

    return vm;

    //#region Internal Methods
    function activate() {
        logger.log('Servers View Activated', null, 'servers', true);
        return true;
    }
    //#endregion
});