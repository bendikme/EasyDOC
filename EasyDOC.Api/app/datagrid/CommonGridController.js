var CommonGridController = BaseController.extend({

	_datacontext: null,
	_entityInfo: null,
	_gridController: null,
	_modal: null,
	_mode: null,
	_notifications: null,
	_isModal: false,
	_permission: null,
	_settings: null,
	_toolbar: null,
	_type: null,
	_uniqueKey: null,

	$rootScope: null,
	$scope: null,
	$state: null,

	/**
	 * Initialize Notes Controller
	 * @param $scope, current controller scope
	 */
	init: function ($rootScope, $scope, $state, Notifications, datacontext, modal, permissions, settings) {

		this.$rootScope = $rootScope;
		this.$scope = $scope;
		this.$state = $state;

		this._datacontext = datacontext;
		this._modal = modal;
		this._notifications = Notifications;
		this._permissions = permissions;
		this._settings = settings;

		this._super($scope);
	},

	/**
	 *@Override
	 */
	defineListeners: function () {
		this.$scope.$on('$stateChangeStart', this._onStateChangeStart.bind(this));
		this._super();
	},


	/**
	 *@Override
	 */
	defineScope: function () {
		this.$scope.toolbar = _.values(this._toolbar);
	},

	/**
	 *@Override
	 */
	destroy: function () {
		this._super();
		this._saveSettings();
	},

	_saveSettings: function () {

		var settings = {
			sort: this._gridController && this._gridController._sortedColumns || []
		};

		_.each(this._gridController._columns, function (column) {
			settings[column.property] = {
				filters: column.filterCollection.getFilters(),
				sort: column.sort,
				visible: column.visible
			};
		});

		localStorage.setItem(this._uniqueKey, JSON.stringify(settings));
	},

	_loadSettings: function (columns) {
		var settings = JSON.parse(localStorage.getItem(this._uniqueKey));

		if (settings) {
			_.each(settings, function (value, key) {

				var column = _.findWhere(columns, { property: key });

				if (column) {
					column.visible = value.visible;
					column.sort = value.sort;
					column.filterCollection = new FilterCollection(column);
					_.each(value.filters, function (filter) {
						column.filterCollection.add(column.property || column.name, filter.bool, filter.op, filter.value);
					});

					var filters = column.filterCollection.getFilters();
					if (filters.length > 0) {
						column.filter = filters[0].value;
					}

				} else {
					localStorage.removeItem(this._uniqueKey);
				}
			});

			this._gridController._sortedColumns = settings.sort;
		}
	},

	_onStateChangeStart: function (event, toState, toParams, fromState) {
		this._saveSettings();

		if (this._datacontext.hasNewItems() || this._isModal) {
			if (!this._isModal) {
				toastr.warning('Save new items before leaving page', 'Warning!');
			}
			event.preventDefault();
		}
	},

	_saveChanges: function () {
		this._datacontext.saveChanges();
	},

	_cancelChanges: function () {
		this._datacontext.cancelChanges();
	},

	_resetGrid: function () {
		this._gridController.reset();
	},

	_linkToParent: function () {

		var items = this._gridController.selection.getSelectedItems();

		if (this._isModal) {
			if (this.$state.params.CKEditorFuncNum) {
				var file = items[0];
				window.opener.CKEDITOR.tools.callFunction(this.$state.params.CKEditorFuncNum, this.$scope.getUrl(file));
				window.close();
			} else {
				if (this._datacontext.hasNewItems()) {
					toastr.warning('Save item before linking.', 'Warning!');
					return;
				}
				this._modal.postMessageAndCloseWindow({ type: this._type, ids: _.pluck(items, 'Id') });
			}
		}
	},

	_getLinkedItemsFilter: function () {
		return [this._entityInfo.resource, this._entityInfo.fromId, this._entityInfo.toId, this._parentId].join('|');
	},

});