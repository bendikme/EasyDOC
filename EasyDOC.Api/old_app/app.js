(function () {
    toastr.options.showMethod = 'slideDown';
    toastr.options.hideMethod = 'slideUp';
    toastr.options.positionClass = 'toast-bottom-right';

    var app = angular.module('app', [
        'notifications',
        'commands',
        'datagrid',
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

    app.config(function ($logProvider, $locationProvider) {
        $locationProvider.html5Mode(true);
    });

    app.run(function ($rootScope, $q, breeze, common, toolbar, editableOptions, permissions) {
        common.setBusy(true);
        editableOptions.theme = 'bs3';

        permissions.whenReady(function () {
            common.setBusy(false);
        });

        $rootScope.loading = false;

        common.setBusyCallback(function () {
            $rootScope.loading = common.getBusy();
            _.defer(function () {
                $rootScope.$apply();
            });
        });

        if (window.opener) {
            $rootScope.isModalWindow = true;
        }

        $rootScope.$on('$stateChangeSuccess', function (event, toState) {
            $rootScope.title = 'EasyDOC ' + toState.title;
        });

        $rootScope.$on('$stateChangeStart', function () {
            toolbar.items = [];
        });
    });

    app.factory('toolbar', function () {
        return { items: [] };
    });

    app.filter('range', function () {
        return function (input) {
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
        };
    });
})();
//# sourceMappingURL=app.js.map
