var DashboardController = BaseController.extend({
    _datacontext: null,
    _notifications: null,
    _permissions: null,

    $rootScope: null,

    init: function ($rootScope, $scope, Notifications, datacontext, permissions) {

        this._datacontext = datacontext;
        this._notifications = Notifications;
        this._permissions = permissions;

        this.$rootScope = $rootScope;

        this._super($scope);

        var that = this;

        permissions.whenReady(function () {
            that._loadActiveProjects.apply(that);
            that._loadExperiences.apply(that);
            that._loadTasks.apply(that);
        });
    },

    defineScope: function () {
        this._super();
        this.$scope.experiences = null;
        this.$scope.employeeTasks = null;

        this.$scope.activeProjectCount = 0;
        this.$scope.tasksOverdue = 0;
        this.$scope.tasksOverdueThisWeek = 0;

        this.$scope.loadExperiences = this._loadExperiences.bind(this);
        this.$scope.loadTasks = this._loadTasks.bind(this);

        this.$scope.getDate = function () {
            return new Date();
        }
    },

    defineListeners: function () {
        this._super();
    },

    _loadExperiences: function () {
        var that = this;
        var query = this._datacontext.createQuery().from('Experiences').select('Id, Name, Creator').take(10);
        this._datacontext.executeQuery(query)
			.then(function (data) {
			    that.$scope.experiences = data.results;
			});
    },

    _loadActiveProjects: function () {
        var that = this;
        var query = this._datacontext.createQuery().from('Projects').select('Id').where('Status', '==', 'Active').take(0).inlineCount();
        this._datacontext.executeQuery(query)
			.then(function (data) {
			    that.$scope.activeProjectCount = data.inlineCount;
			});
    },

    _loadTasks: function () {
        var that = this;
        var query = this._datacontext.createQuery()
			.from('EmployeeTasks')
			.expand('Task,Task.Project')
			//.where('EmployeeId', '==', this._permissions.getCurrentUserId())
			.where('Task.PercentComplete', '<', 100);

        this._datacontext.executeQuery(query)
			.then(function (data) {
			    Metronic.initSlimScroll('.scroller');
			    that.$scope.employeeTasks = data.results;
			});
    },

});

angular.module('app').controller('DashboardController', ['$rootScope', '$scope', 'Notifications', 'datacontext', 'permissions', DashboardController]);