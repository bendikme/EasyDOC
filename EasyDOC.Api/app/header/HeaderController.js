/**
 * Base controller for all controllers.
 * Use this as a template for all future controllers
 *
 * Use of Class.js
 */
var HeaderController = BaseController.extend({

	init: function (scope) {
		this._super(scope);
	},


	defineListeners: function () {
		this._super();
	},


	defineScope: function () {
		this._super();
	}

});

angular.module('app').controller('HeaderController', ['$scope', HeaderController]);