(function () {
	'use strict';

	angular.module('app').config(function ($stateProvider, $urlRouterProvider) {

		$urlRouterProvider.otherwise('/');

		$stateProvider.state('home', {
			url: '/',
			views: {
				"main": {
					templateUrl: '/app/dashboard/dashboard.html',
					controller: 'DashboardController'
				}
			},
			title: 'Hjem'
		});

		/*$stateProvider.state('project', {
		    url: '/project',
		    views: {
			"main": {
			    templateUrl: '/app/project/overview.html',
			    controller: 'ProjectController'
			}
		    },
		    title: 'Prosjekter'
		});
	
		$stateProvider.state('projectDetails', {
		    url: '/project/{projectnumber:[A-Z]+[0-9]+-[0-9]+}',
		    views: {
			"main": {
			    templateUrl: '/app/project/details.html',
			    controller: 'ProjectController'
			}
		    },
		    title: 'Prosjekter'
		});
	
		$stateProvider.state('registerOrderConfirmation', {
		    url: '/project/{projectnumber:[A-Z]+[0-9]+-[0-9]+}/registerOrderConfirmation',
		    views: {
			"main": {
			    templateUrl: '/app/project/registerOrderConfirmation.html',
			    controller: 'ProjectController'
			}
		    },
		    title: 'Prosjekter'
		});
	
		$stateProvider.state('orderConfirmations', {
		    url: '/project/{projectnumber:[A-Z]+[0-9]+-[0-9]+}/orderConfirmations/{id:[0-9]+}',
		    views: {
			"main": {
			    templateUrl: '/app/project/orderConfirmation.html',
			    controller: 'OrderConfirmationController'
			}
		    },
		    title: 'Prosjekter'
		});*/

		$stateProvider.state('folder', {
			url: '/folder/{id:[0-9]+}?mode&filter&CKEditor&CKEditorFuncNum&langCode',
			views: {
				"main": {
					templateUrl: '/app/tree/tree.html'
				}
			},
			title: 'Browser'
		});

		$stateProvider.state('category', {
			url: '/category/{id:[0-9]+}?mode',
			views: {
				"main": {
					templateUrl: '/app/tree/tree.html'
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
					template: '<ganttchart items="items"></ganttchart>'
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
