
(function () {

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
        'pasvaz.bindonce'
    ]);


    app.config(function ($logProvider, $locationProvider) {
        $locationProvider.html5Mode(true);
        $logProvider.debugEnabled(true);
    });

    app.run(function ($rootScope, $q, breeze, common, toolbar, editableOptions) {
        editableOptions.theme = 'bs3';

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

        $rootScope.$on('$stateChangeSuccess', function (event, toState, toParams, fromState, fromParams) {
            $rootScope.title = 'EasyDOC ' + toState.title;
        });

        $rootScope.$on('$stateChangeStart', function(event, toState, toParams, fromState, fromParams) {
            toolbar.items = [];
        });
    });

    angular.module('app').factory('toolbar', function () {
        return { items: [] };
    });

    angular.module('app').filter('range', function () {
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