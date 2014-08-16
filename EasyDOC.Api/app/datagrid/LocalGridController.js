(function () {

    var LocalGridController = AbstractGridController.extend({
        _entityInfo: null,

        /**
	 * Initialize Notes Controller
	 * @param $scope, current controller scope
	 */
        init: function ($rootScope, $scope, $state, Notifications, permissions, common) {
            this._super($rootScope, $scope, Notifications, permissions, common);
        },

        /**
	 *@Override
	 */
        defineListeners: function () {
            this._super();
            this.$scope.$emit('gridReady', this);
            this._notifications.addEventListener('itemsLoaded', this._updateItemsWithTemplate.bind(this));

            var that = this;
            this._notifications.addEventListener('newItemAdded', function (event, item) {
                that._updateItemsWithTemplate(event, [item]);
            });
        },

        /**
	 *@Override
	 */
        defineScope: function () {
            this._super();
        },

        _updateItemsWithTemplate: function (event, items) {
            _.each(items, function (item) {
                this._extendItemWithTemplate(item);
            }, this);
        },

        _setConfiguration: function (config) {
            this._parentId = config[0];
            this._configuration = config[1];
            this._columns = config[2];
            this._entityInfo = config[3];
            this._parentController = config[4];
            this.$scope.isReadOnly = config[5];
            this._prepareColumns();
            this._isReady = true;
        }

    });


    angular.module('app').controller('LocalGridController', ['$rootScope', '$scope', '$state', 'Notifications', 'permissions', 'common', LocalGridController]);

})();