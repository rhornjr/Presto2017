define(['services/logger'], function (logger) {
    var vm = {
        activate: activate,
        title: 'Ping'
    };

    return vm;

    //#region Internal Methods
    function activate() {
        logger.log('Ping View Activated', null, 'ping', true);
        return true;
    }
    //#endregion
});