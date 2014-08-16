var OrderConfirmationController = BaseController.extend({

	_datacontext: null,
	_notifications: null,
	_permissions: null,

	$location: null,
	$rootScope: null,
	$state: null,

	init: function ($rootScope, $scope, $state, $location, Notifications, datacontext, permissions) {

		this._datacontext = datacontext;
		this._notifications = Notifications;
		this._permissions = permissions;

		this.$location = $location;
		this.$rootScope = $rootScope;
		this.$state = $state;

		this._super($scope);

		if ($state.params.id) {
			this._loadOrderConfirmation($state.params.id);
		}

	},

	defineScope: function () {

		this._super();
		this.$scope.item = null;
		this.$scope.projectManager = null;
		this.$scope.customer = null;
		this.$scope.project = null;

		this.$scope.employees = [];
		this.$scope.customers = [];
		this.$scope.projects = [];

		this.$scope.saveChanges = function () {
			this._datacontext.saveChanges(null, function (error) {
				toastr.error(error.message, 'Noe gikk galt!');
			});
		}.bind(this);

		this.$scope.hasChanges = this._datacontext.hasChanges;
		this.$scope.cancelChanges = this._datacontext.cancelChanges;

		this.$scope.formatProject = function (project) {
			if (project)
				return project.ProjectNumber + ' - ' + project.Name + ' (' + project.Customer.Name + ')';
		}
	},

	defineListeners: function () {
		var that = this;

		this._super();
		this.$scope.$watch('projectManager', this._getEmployees.bind(this));
		this.$scope.$watch('customer', this._getCustomers.bind(this));
		this.$scope.$watch('project', this._getProjects.bind(this));

		this.$scope.$watch('item.ProjectManager', function (value) {
			if (value) {
				that.$scope.projectManager = value.Name;
			} else {
				that.$scope.projectManager = null;
			}
		});

		this.$scope.$watch('item.Customer', function (value) {
			if (value) {
				that.$scope.customer = value.Name;
			} else {
				that.$scope.customer = null;
			}
		});
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

			this._datacontext.executeQuery(query)
				.then(function (data) {
					that.$scope.projects = data.results || [];
				}).catch(function (error) {
					console.error(error);
				}).finally(function () {
					that.$scope.loadingProjects = false;
				});
		}
	},

	_getEmployees: function (search) {

		if (_.isString(search) && search.length > 0) {

			this.$scope.loadingEmployees = true;

			var query = this._datacontext.createQuery()
			    .from('Employees')
			    .where('Name', 'Contains', search)
			    .orderBy('Name')
			    .take(10);

			var that = this;

			this._datacontext.executeQuery(query)
				.then(function (data) {
					that.$scope.employees = data.results || [];
				}).catch(function (error) {
					console.error(error);
				}).finally(function () {
					that.$scope.loadingEmployees = false;
				});
		}
	},

	_getCustomers: function (search) {

		if (_.isString(search) && search.length > 0) {

			this.$scope.loadingCustomers = true;

			var query = this._datacontext.createQuery()
			    .from('Customers')
			    .where('Name', 'Contains', search)
			    .orderBy('Name')
			    .take(10);

			var that = this;

			this._datacontext.executeQuery(query)
				.then(function (data) {
					that.$scope.customers = data.results || [];
				}).catch(function (error) {
					console.error(error);
				}).then(function () {
					that.$scope.loadingCustomers = false;
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

	_loadOrderConfirmation: function (id) {

		var that = this;

		$('.date-picker').datepicker({
			autoclose: true,
			format: 'dd.mm.yyyy',
			weekStart: 1,
			language: 'no'
		}).on('changeDate', function (ev) {
			that._updateScope(function () {
				that.$scope.item.StartDate = ev.date;
			});
		});

		var query = this._datacontext.createQuery()
		    .from('OrderConfirmations')
		    .expand('Creator,Editor,Project,Project.Customer,Items,Vendor,CustomerReference,Items.Component')
		    .where('Id', '==', id);

		this.$scope.loadingOrderConfirmation = true;

		this._datacontext.executeQuery(query)
			.then(function (data) {
				that.$scope.item = data.results[0];
			}).catch(function (error) {
				console.log(error);
			}).finally(function () {
				that.$scope.loadingOrderConfirmation = false;
			});
	}
});

angular.module('app').controller('OrderConfirmationController', ['$rootScope', '$scope', '$state', '$location', 'Notifications', 'datacontext', 'permissions', OrderConfirmationController]);