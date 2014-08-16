var ProjectController = BaseController.extend({
	_datacontext: null,
	_notifications: null,
	_permissions: null,

	$location: null,
	$rootScope: null,
	$state: null,

	_queryId: 0,

	init: function ($rootScope, $scope, $state, $location, Notifications, datacontext, permissions) {

		this._datacontext = datacontext;
		this._notifications = Notifications;
		this._permissions = permissions;

		this.$location = $location;
		this.$rootScope = $rootScope;
		this.$state = $state;

		this._super($scope);

		if ($state.params.projectnumber) {
			this._loadProject($state.params.projectnumber);
		}

	},

	defineScope: function () {

		this._super();
		this.$scope.item = null;
		this.$scope.projectManager = null;

		this.$scope.orderConfirmationRegistered = true;
		this.$scope.registerOrderConfirmationStep = 1;

		this.$scope.saveChanges = function () {
			this._datacontext.saveChanges(null, function (error) {
				toastr.error(error.message, 'Noe gikk galt!');
			});
		}.bind(this);

		this.$scope.hasChanges = this._datacontext.hasChanges;
		this.$scope.cancelChanges = this._datacontext.cancelChanges;

		this.$scope.selectProject = this._selectProject.bind(this);
		this.$scope.setRegisterOrderConfirmationStep = this._setRegisterOrderConfirmationStep.bind(this);

		this.$scope.formatProject = function (project) {
			if (project)
				return project.ProjectNumber + ' - ' + project.Name + (project.Customer ? (' (' + project.Customer.Name + ')') : '');
		}
	},

	defineListeners: function () {
		var that = this;

		this._super();

		this.$scope.getAsync = this._getAsync.bind(this);
		this.$scope.getProjects = this._getProjects.bind(this);

		this.$scope.$watch('item.ProjectManager', function (value) {
			that.$scope.projectManager = value ? value.Name : null;
		});

		this.$scope.$watch('item.StartDate', function (value) {
			that.$scope.startDate = moment(value).format('DD.MM.YYYY');
		});

		this.$scope.$watch('item.Customer', function (value) {
			that.$scope.customer = value ? value.Name : null;
		});
	},

	_setRegisterOrderConfirmationStep: function (step) {
		this.$scope.registerOrderConfirmationStep = step;
	},

	_getProjects: function (search) {

		if (_.isString(search) && search.length > 0) {

			this.$scope.loadingProjects = true;

			var query = this._datacontext.createQuery()
			    .from('Projects')
			    .expand('Customer')
			    .orderByDesc('ProjectNumber')
			    .take(10);

			query = query.where(this._getPredicate(search, 'ProjectNumber,Name,Customer.Name'));

			var that = this;

			return this._datacontext.executeQuery(query)
				.finally(function () {
					that.$scope.loadingProjects = false;
				}).then(function (data) {
					return data.results;
				});
		}
	},

	_getAsync: function (from, search) {

		if (_.isString(search) && search.length > 0) {

			this.$scope.loadingCustomers = true;

			var query = this._datacontext.createQuery()
			    .from(from)
			    .where('Name', 'Contains', search)
			    .orderBy('Name')
			    .take(10);

			var that = this;

			return this._datacontext.executeQuery(query)
				.finally(function () {
					that.$scope.loadingCustomers = false;
				}).then(function (data) {
					return data.results;
				});
		}
	},

	_getPredicate: function (term, properties) {

		var queries = term.split(' ');
		var andPredicate = null;

		var props = properties.split(',');

		_.each(queries, function (q) {

			var orPredicate = null;

			_.each(props, function (property) {
				var p = new breeze.Predicate(property, 'Contains', q);
				orPredicate = orPredicate ? orPredicate.or(p) : p;
			});

			andPredicate = andPredicate ? andPredicate.and(orPredicate) : orPredicate;
		});

		return andPredicate;
	},

	_selectProject: function (project) {
		this.$location.path('/project/' + project.ProjectNumber);
		this._updateScope();
	},

	_loadProject: function (projectNumber) {

		var that = this;

		$('.date-picker').datepicker({
			autoclose: true,
			format: 'dd.mm.yyyy',
			weekStart: 1,
			language: 'no'
		}).on('changeDate', function (ev) {
			that._updateScope(function () {
				that.$scope.item.StartDate = ev.date;
				that.$scope.startDate = moment(that.$scope.item.StartDate).format('DD.MM.YYYY');
			});
		});

		var query = this._datacontext.createQuery()
		    .from('Projects')
		    .expand('Creator,Editor,ProjectManager,Customer,Components,Maintenances,Files,Safeties,Experiences,Documentations,OrderConfirmations')
		    .where('ProjectNumber', '==', projectNumber);

		this.$scope.loadingProjects = true;

		this._datacontext.executeQuery(query)
			.then(function (data) {
				that.$scope.item = data.results[0];
			}).finally(function () {
				that.$scope.loadingProjects = false;
			});
	}
});

angular.module('app').controller('ProjectController', ['$rootScope', '$scope', '$state', '$location', 'Notifications', 'datacontext', 'permissions', ProjectController]);