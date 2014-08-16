var NavigationController = BaseController.extend({
	_datacontext: null,
	_notifications: null,
	_permissions: null,

	$rootScope: null,
	$state: null,

	_menu: [
	    {
	    	title: 'Hjem',
	    	description: 'oversikt over prosjekter og service',
	    	icon: 'home',
	    	url: '/',
	    	selected: true
	    },
	    {
	    	title: 'Prosjekter',
	    	icon: 'cubes',
	    	subMenu: [
		    {
		    	title: 'Prosjekter',
		    	icon: 'cubes',
		    	url: '/project',
		    },
		    {
		    	title: 'Erfaringer',
		    	icon: 'reddit',
		    	url: '/experience',
		    }
	    	]
	    },
	    {
	    	title: 'Dokumentasjon',
	    	icon: 'file',
	    	subMenu: [
		    {
		    	title: 'Dokumentasjon',
		    	icon: 'file',
		    	url: '/documentation',
		    },
		    {
		    	title: 'Kapitler',
		    	icon: 'files',
		    	url: '/chapter',
		    },
		    {
		    	title: 'Vedlikehold',
		    	icon: 'heart',
		    	url: '/maintenance',
		    },
		    {
		    	title: 'Sikkerhet',
		    	icon: 'ambulance',
		    	url: '/safety',
		    }
	    	]
	    },
	    {
	    	title: 'Komponenter',
	    	icon: 'gear',
	    	subMenu: [
		    {
		    	title: 'Komponenter',
		    	icon: 'gear',
		    	url: '/component',
		    },
		    {
		    	title: 'Serier',
		    	icon: 'gears',
		    	url: '/componentseries',
		    },
		    {
		    	title: 'Kategorier',
		    	icon: 'tree',
		    	url: '/category/1',
		    }
	    	]
	    },
	    {
	    	title: 'Kontakter',
	    	icon: 'user',
	    	subMenu: [
		    {
		    	title: 'Ansatte',
		    	icon: 'users',
		    	url: '/employee',
		    },
		    {
		    	title: 'Kunder',
		    	icon: 'wheelchair',
		    	url: '/customer',
		    },
		    {
		    	title: 'Leverandører',
		    	icon: 'truck',
		    	url: '/vendor',
		    },
	    	]
	    },
	    {
	    	title: 'Filer',
	    	icon: 'file',
	    	url: '/folder/1'
	    },
	    {
	    	title: 'Administrator',
	    	icon: 'gears',
	    	subMenu: [
		    {
		    	title: 'Kontoer',
		    	icon: 'user',
		    	url: '/user'
		    },
		{
			title: 'Roller',
			icon: 'lock',
			url: '/role'
		}
	    	]
	    },
	],

	init: function ($rootScope, $scope, $state, Notifications, datacontext, permissions) {

		this._datacontext = datacontext;
		this._notifications = Notifications;
		this._permissions = permissions;

		this.$rootScope = $rootScope;
		this.$state = $state;

		this._super($scope);

	},

	defineScope: function () {
		this._super();
		this.$scope.menu = this._menu;
	},

	defineListeners: function () {
		this._super();
		this.$scope.$on('$stateChangeStart', this._onStateChangeStart.bind(this));
		this.$scope.path = null;
	},

	_onStateChangeStart: function (event, toState, toParams, fromState) {
		_.each(this._menu, function (item) {
			item.selected = false;
		});

		var item = _.findWhere(this._menu, { url: toState.url });

		if (item) {
			item.selected = true;
		}

		this.$scope.path = null;
	}
});

angular.module('app').controller('NavigationController', ['$rootScope', '$scope', '$state', 'Notifications', 'datacontext', 'permissions', NavigationController]);