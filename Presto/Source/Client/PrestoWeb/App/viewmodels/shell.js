define(['durandal/system', 'durandal/plugins/router', 'services/logger'],
    function (system, router, logger) {
        var shell = {
            activate: activate,
            router: router
        };
        
        return shell;

        //#region Internal Methods
        function activate() {
            return boot();
        }

        function boot() {
            router.mapNav('apps');
            router.mapNav('servers');
            router.mapNav('variables');
            router.mapNav('resolve');
            router.mapNav('installs');
            router.mapNav('log');
            router.mapNav('ping');
            router.mapNav('global');
            router.mapNav('security');
            log('Hot Towel SPA Loaded!', null, true);
            return router.activate('apps');
        }

        function log(msg, data, showToast) {
            logger.log(msg, data, system.getModuleId(shell), showToast);
        }
        //#endregion
    });