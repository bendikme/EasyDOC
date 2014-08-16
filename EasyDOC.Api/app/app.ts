
(() => {

    toastr.options.showMethod = 'slideDown';
    toastr.options.hideMethod = 'slideUp';
    toastr.options.positionClass = "toast-bottom-right";

    var app = angular.module('app', [
        'ui.router',
        'ui.bootstrap',
        'ui.sortable',
        'angularMoment',
        'breeze.angular',
        'breeze.directives',
        'xeditable',
        'ngTagsInput',
        'pasvaz.bindonce'
    ]);


    app.config(($logProvider, $locationProvider) => {
        $locationProvider.html5Mode(true);
    });

    app.run(($rootScope, $q, breeze, common, toolbar, editableOptions, permissions) => {
        common.setBusy(true);
        editableOptions.theme = 'bs3';

        permissions.whenReady(() => {
            common.setBusy(false);
        });

        $rootScope.loading = false;

        common.setBusyCallback(() => {
            $rootScope.loading = common.getBusy();
            _.defer(() => {
                $rootScope.$apply();
            });
        });

        if (window.opener) {
            $rootScope.isModalWindow = true;
        }

        $rootScope.$on('$stateChangeSuccess', (event, toState) => {
            $rootScope.title = 'EasyDOC ' + toState.title;
        });

        $rootScope.$on('$stateChangeStart', () => {
            toolbar.items = [];
        });
    });

    app.factory('toolbar', () => {
        return { items: [] };
    });

    app.filter('range', () => input => {
        var lowBound, highBound;
        switch (input.length) {
            case 1:
                lowBound = 0;
                highBound = parseInt(input[0]) - 1;
                break;
            case 2:
                lowBound = parseInt(input[0]);
                highBound = parseInt(input[1]);
                break;
            default:
                return input;
        }

        return _.range(lowBound, highBound);
    });

})();