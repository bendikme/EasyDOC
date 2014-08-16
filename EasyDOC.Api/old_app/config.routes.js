(function () {
    'use strict';

    angular.module('app').config(function ($stateProvider, $urlRouterProvider) {

        $urlRouterProvider.otherwise('/dashboard');

        $stateProvider.state('dashboard', {
            url: '/dashboard',
            views: {
                "main": {
                    templateUrl: '/app/partials/dashboard.html',
                    controller: 'DashboardController'
                }
            },
            title: 'Dashboard'
        });

        $stateProvider.state('folder', {
            url: '/folder/{id:[0-9]+}?mode&filter&CKEditor&CKEditorFuncNum&langCode',
            views: {
                "main": {
                    templateUrl: '/app/partials/file-browser.html',
                    controller: 'browser'
                }
            },
            title: 'Browser'
        });

        $stateProvider.state('category', {
            url: '/category/{id:[0-9]+}?mode',
            views: {
                "main": {
                    templateUrl: '/app/partials/file-browser.html',
                    controller: 'browser'
                }
            },
            title: 'Browser'
        });

        $stateProvider.state('import', {
            url: '/import',
            views: {
                'main': {
                    templateUrl: '/app/partials/import.html',
                    controller: 'ImportController'
                }
            },
            title: 'Import'
        });

        $stateProvider.state('search', {
            url: '/search?query',
            views: {
                'main': {
                    templateUrl: '/app/partials/search.html',
                    controller: 'SearchController'
                }
            },
            title: 'Search'
        });

        $stateProvider.state('projectTasks', {
            url: '/project/tasks/{id:[0-9]+}',
            views: {
                'main': {
                    templateUrl: '/app/partials/tasks.html'
                }
            },
            title: 'Tasks'
        });

        $stateProvider.state('list', {
            url: '/{type:[a-z]+}?mode&filter',
            views: {
                'main': {
                    templateUrl: '/app/partials/list.html',
                    controller: 'CompleteGridController'
                }
            },
            title: 'List'
        });

        $stateProvider.state('details', {
            url: '/{maintype:[a-z]+}/{id:[0-9]+}',
            views: {
                'main': {
                    templateUrl: '/app/partials/details.html',
                    controller: 'details'
                }
            },
            title: 'Details'
        });

        $stateProvider.state('details.list', {
            url: '/{type:[a-z]+}',
            views: {
                'sub': {
                    templateUrl: '/app/partials/list.html',
                    controller: 'CompleteGridController'
                }
            },
            title: 'Details'
        });
    });
})();
//# sourceMappingURL=config.routes.js.map
