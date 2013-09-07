define(['services/logger'], function (logger) {
    var vm = {
        activate: activate,
        title: 'Resolve'
    };

    return vm;

    //#region Internal Methods
    function activate() {
        logger.log('Resolve View Activated', null, 'resolve', true);
        return true;
    }
    //#endregion
});