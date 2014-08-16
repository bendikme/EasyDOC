/**
 * Base controller for all controllers.
 * Use this as a template for all future controllers
 *
 * Use of Class.js
 */
var BaseController = Class.extend({
    $scope: null,
    _listeners: {},


    /**
	     * Initialize Base Controller
	     * @param $scope, current controller scope
	     */
    init: function (scope) {
        this.$scope = scope;
        this.defineListeners();
        this.defineScope();
    },


    /**
	     * Initialize listeners needs to be overrided by the subclass.
	     * Don't forget to call _super() to activate
	     */
    defineListeners: function () {
        this.$scope.$on('$destroy', this.destroy.bind(this));

        _.each(this._listeners, function (value, key) {
            this._notifications.addEventListener(key, value);
        }, this);
    },


    /**
	     * Use this function to define all scope objects.
	     * Give a way to instantaly view whats available
	     * publicly on the scope.
	     */
    defineScope: function () {
        //OVERRIDE
    },


    /**
		 * Triggered when controller is about
		 * to be destroyed, clear all remaining values.
		 */
    destroy: function (event) {
        _.each(this._listeners, function (value, key) {
            this._notifications.removeEventListener(key, value);
        }, this);
    },

    _updateScope: function (func) {
        if (!this.$scope.$$phase) {
            this.$scope.$apply(func);
        } else {
            func.apply();
        }
    }
});


BaseController.$inject = ['$scope'];