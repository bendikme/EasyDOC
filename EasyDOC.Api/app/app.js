(function () {

	toastr.options.showMethod = 'slideDown';
	toastr.options.hideMethod = 'slideUp';
	toastr.options.positionClass = 'toast-bottom-right';

	var app = angular.module('app', [
	    'angularMoment',
	    'breeze.angular',
	    'breeze.directives',
	    'commands',
	    'datagrid',
	    'ngTagsInput',
	    'notifications',
        'sly',
	    'ui.router',
	    'ui.router.router',
	    'ui.bootstrap'
	]);

	app.config(function ($locationProvider, $urlRouterProvider) {
		$locationProvider.html5Mode(true);
		//$urlRouterProvider.deferIntercept();
	});

	app.run(function ($rootScope, $q, breeze, amMoment) {

	    amMoment.changeLanguage('nb');

		if (window.opener) {
			$rootScope.isModalWindow = true;
		}

		$rootScope.$on('$stateChangeSuccess', function (event, toState) {
			$rootScope.title = 'EasyDOC | ' + toState.title;
		});

		//$urlRouter.sync();
	});
})();

var countWatches = function () {
    var elts = document.getElementsByClassName('ng-scope');
    var watches = [];
    var visited_ids = {};
    for (var i = 0; i < elts.length; i++) {
        var scope = angular.element(elts[i]).scope();
        if (scope.$id in visited_ids)
            continue;
        visited_ids[scope.$id] = true;
        watches.push.apply(watches, scope.$$watchers);
    }
    return watches.length;
};