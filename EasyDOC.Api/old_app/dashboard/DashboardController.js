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

    },

    defineScope: function () {
        this._super();
    },

    defineListeners: function () {
        this._super();
    },

    _onUpdateButtonStates: function () {

    },

    _updateScope: function () {
        if (!this.$scope.$$phase && !this.$scope.$root.$$phase) {
            this.$scope.$apply();
        }
    }
});

DashboardController.$inject = ['$rootScope', '$scope', 'Notifications', 'datacontext', 'permissions'];