define(['services/logger'], function (logger) {
    var vm = {
        activate: activate,
        title: 'Installs'
    };

    return vm;

    //#region Internal Methods
    function activate() {
        logger.log('Installs View Activated', null, 'installs', true);
        return true;
    }
    //#endregion
});