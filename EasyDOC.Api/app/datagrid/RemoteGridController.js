(function () {

    var RemoteGridController = AbstractGridController.extend({
        _datacontext: null,
        _entityInfo: null,

        _paging: {
            current: 0,
            showStart: 0,
            showEnd: 0
        },

        _queryParams: {
            skip: 0,
            take: 25
        },

        /**
		 * Initialize Notes Controller
		 * @param $scope, current controller scope
		 */
        init: function ($rootScope, $scope, $state, Notifications, datacontext, permissions, common) {
            this._datacontext = datacontext;
            this._super($rootScope, $scope, Notifications, permissions, common);
        },

        /**
		 *@Override
		 */
        defineListeners: function () {
            this._super();
            this.$scope.$watch('queryParams.take', this._onQueryParamsChanged.bind(this));
            this.$scope.$emit('gridReady', this);
        },

        /**
		 *@Override
		 */
        defineScope: function () {
            this._super();

            this.$scope.paging = this._paging;
            this.$scope.queryParams = this._queryParams;
            this.$scope.gotoPage = this._gotoPage.bind(this);
            this.$scope.getAsync = this._getAsync.bind(this);
        },

        reset: function () {
            if (this._datacontext.hasNewItems()) {
                toastr.warning('Save new items before updating page', 'Warning!');
            } else {
                _.each(this._columns, (function (column) { column.filterCollection.clear(); }));
                this._queryDatabase();
            }
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
        },

        _load: function () {
            this._super();
            this._queryDatabase();
        },

        _queryDatabase: function () {
            this._clearSelection();
            this._datacontext.executeQuery(this._getQuery())
			    .then(this._querySucceeded.bind(this))
			    .catch(this._queryFailed.bind(this));
        },

        _getQuery: function () {

            var orderBy = this._entityInfo.sort || 'Name';

            if (this._sortedColumns.length) {
                orderBy = _.reduce(this._sortedColumns, function (memo, column) {
                    return memo + column.property + (column.direction < 0 ? ' desc' : '') + ',';
                }, '');

                orderBy = orderBy.substring(0, orderBy.length - 1);
            } else {
                this._sortedColumns[0] = {
                    property: this._entityInfo.sort || 'Name',
                    direction: 1
                };
            }

            var query = this._datacontext.createQuery()
			    .from(this._entityInfo.resource)
			    .orderBy(orderBy)
			    .expand(this._entityInfo.expand)
			    .skip(this._queryParams.skip)
			    .take(this._queryParams.take)
			    .inlineCount();

            var predicate = null;
            predicate = this.filters.apply(predicate);

            _.each(this._columns, function (column) {
                predicate = column.filterCollection.apply(predicate);
            });

            return query.where(predicate);
        },

        _querySucceeded: function (data) {

            this.$scope.count = data.inlineCount;

            this.$scope.items = _.map(data.results, function (item) {
                return this._extendItemWithTemplate(item);
            }, this);

            this._paging.current = Math.floor(this._queryParams.skip / this._queryParams.take) + 1;
            this._paging.showStart = this._queryParams.skip + 1;
            this._paging.showEnd = Math.min(this._queryParams.skip + this._queryParams.take, data.inlineCount);
        },

        _queryFailed: function (error) {
            toastr.error('Error fetching records from server.', 'Error!');
            console.error(error);
        },

        _gotoPage: function (page) {
            if (this._datacontext.hasNewItems()) {
                toastr.warning('Save new items before updating page', 'Warning!');
            } else {
                this._queryParams.skip = this._queryParams.take * (page - 1);
                this._queryDatabase();
            }
        },

        _onQueryParamsChanged: function () {

            if (this._datacontext.hasNewItems()) {
                toastr.warning('Save new items before updating page', 'Warning!');
            } else if (this._isReady) {
                this._queryParams.take = parseInt(this.$scope.queryParams.take);
                this._queryDatabase();
            }
        },

        _orderByProperty: function (property, event) {
            if (this._datacontext.hasNewItems()) {
                toastr.warning('Save new items before reordering', 'Warning!');
                return;
            }

            this._super(property, event);
            this._queryDatabase();
        },

        _refresh: function () {
            if (this._datacontext.hasNewItems()) {
                toastr.warning('Save new items before updating page', 'Warning!');
            } else {
                this._queryDatabase();
            }
        },

    });

    angular.module('app').controller('RemoteGridController', ['$rootScope', '$scope', '$state', 'Notifications', 'datacontext', 'permissions', 'common', RemoteGridController]);
})();