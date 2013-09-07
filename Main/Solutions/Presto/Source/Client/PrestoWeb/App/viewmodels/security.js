define(['services/logger'], function (logger) {
    var vm = {
        activate: activate,
        title: 'Security'
    };

    return vm;

    //#region Internal Methods
    function activate() {
        logger.log('Security View Activated', null, 'security', true);
        return true;
    }
    //#endregion
});