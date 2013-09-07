define(['services/logger'], function (logger) {
    var vm = {
        activate: activate,
        title: 'Variables'
    };

    return vm;

    //#region Internal Methods
    function activate() {
        logger.log('Variables View Activated', null, 'variables', true);
        return true;
    }
    //#endregion
});