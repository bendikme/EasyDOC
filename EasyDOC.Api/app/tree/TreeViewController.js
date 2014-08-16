(function () {

	var TreeViewController = CommonGridController.extend({
		_childEntityInfo: null,
		_childConfig: null,

		_firstRootQuery: null,
		_parentId: null,

		_sourceItem: null,
		_canDrop: false,
		_isUploading: false,
		_delayedExpandItem: null,
		_progressId: 0,

		_itemBeingEdited: null,

		$location: null,
		$timeout: null,

		/**
		 * Initialize Notes Controller
		 * @param $scope, current controller scope
		 */
		init: function ($rootScope, $scope, $state, $location, $timeout, Notifications, datacontext, modal, permissions, settings) {

			// TODO: move this to a directive!!!
			var updateTableHeight = _.debounce(function () {
				var top = $("#tree-grid").offset().top;
				var height = $(window).height();
				$("#tree-grid").height(height - top - 120);
			}, 500);

			$(function () {
				updateTableHeight();

				$(window).resize(function () {
					updateTableHeight();
				});
			});

			var that = this;

			this.$location = $location;
			this.$timeout = $timeout;

			this._parentId = parseInt($state.params.id);
			var type = $state.current.name;

			this._entityInfo = _.findWhere(settings.queryParams, { route: type });
			this._childEntityInfo = _.findWhere(settings.queryParams, { route: this._entityInfo.childRoute });
			this._childConfig = settings.detailViewSettings[type];

			var columns = this._childConfig.columns;

			this._mode = $state.params.mode;
			this._isModal = !!this._mode;
			this._type = type;
			this._uniqueKey = this._entityInfo.resource + (this._isModal ? '-select' : ('-' + window.location.pathname));

			// Extend columns with audit information fields as defined in the settings
			if (!_.findWhere(columns, { name: 'Created' }) && !this._childConfig.skipAudit) {
				_.each(settings.detailViewSettings.auditDetails.columns, function (auditProperty) {
					columns.push(auditProperty);
				}, this);
			}

			$scope.$on('gridReady', function (event, gridController) {
				that._gridController = gridController;
				that._gridController.filters.add(that._entityInfo.childParentPropertyId, FilterCollection.booleanAnd.value, 'Equals', that._parentId);
				that._gridController._setConfiguration([that._parentId, that._childConfig, columns, that._childEntityInfo, that]);
				that._updateButtonStates();
			});

			$rootScope.$watch(this._updateButtonStates.bind(this));

			this._toolbar = {
				createParentButton: { order: 0, action: this._createParent.bind(this), icon: 'fa-folder', tooltip: { title: 'Opprett mappe' }, visible: false },
				deleteParentButton: { order: 1, action: this._deleteParent.bind(this), icon: 'fa-trash-o', tooltip: { title: 'Slett mappe' }, visible: false },
				linkChildButton: { order: 2, action: this._linkToParent.bind(this), icon: 'fa-link', tooltip: { title: 'Link fil' }, visible: false },
				saveButton: { order: 3, action: this._saveChanges.bind(this), icon: 'fa-save', tooltip: { title: 'Lagre' }, visible: false },
				cancelButton: { order: 4, action: this._cancelChanges.bind(this), icon: 'fa-rotate-left', tooltip: { title: 'Avbryt' }, visible: false }
			};

			permissions.whenReady(this._setButtonVisibility.bind(this));

			this._super($rootScope, $scope, $state, Notifications, datacontext, modal, permissions, settings);

			datacontext.executeQuery(this._getQuery(this._parentId))
			    .then(this._rootLoaded.bind(that))
			    .catch(this._rootNotLoaded.bind(that));

			$scope.$on('$stateChangeStart', function (e, toState, toParams, fromState, fromParams) {
				if (toState.name == fromState.name) {
					e.preventDefault();
				}
			});

		},

		/**
		 *@Override
		 */
		defineListeners: function () {
			this._super();
		},


		/**
		 *@Override
		 */
		defineScope: function () {

			this.$scope.isLoadingRoot = true;

			this.$scope.toolbar = _.values(this._toolbar);
			this.$scope.toggleChildren = this._toggleChildren.bind(this);
			this.$scope.selectItem = this._selectItem.bind(this);
			this.$scope.getSubItems = this._getSubItems.bind(this);

			// DnD
			this.$scope.onDropFiles = this._onDropFiles.bind(this);
			this.$scope.onDrop = this._onDrop.bind(this);
			this.$scope.onDragStart = this._onDragStart.bind(this);
			this.$scope.onDragEnd = this._onDragEnd.bind(this);
			this.$scope.onDragEnter = this._onDragEnter.bind(this);
			this.$scope.onDragLeave = this._onDragLeave.bind(this);
			this.$scope.onDragOver = this._onDragOver.bind(this);

			this.$scope.startEditingItem = this._startEditingItem.bind(this);
			this.$scope.stopEditingItem = this._stopEditingItem.bind(this);
			this.$scope.saveEditedItem = this._saveEditedItem.bind(this);

			// Upload files
			//this.$scope.uploadFiles = this._uploadFiles(this);

			this.$scope.files = [];
		},

		_toggleChildren: function (item) {
			if (item.entityAspect.entityState.name === 'Unchanged') {
				if (!item.isLoaded) {
					this._loadSubItems(item);
				} else {
					item.isExpanded = !item.isExpanded;
				}
			} else {
				toastr.warning('Save in order to access this item', 'Warning!');
			}
		},

		_getSubItems: function (item) {
			return item && item[this._entityInfo.childrenProperty];
		},

		_loadSubItems: function (item) {
			item.isLoading = true;
			this._querySubItems(item);
		},

		_querySubItems: function (parent) {

			var query = this._getQuery(parent.Id);
			var that = this;

			this._datacontext.executeQuery(query)
			    .then(function (data) {
			    	parent = data.results[0];
			    	parent.isLoading = false;
			    	parent.isLoaded = true;
			    	parent.hasChildren = parent[that._entityInfo.childrenProperty].length > 0;
			    	parent.isExpanded = parent.hasChildren;
			    	that._extendWithViewModelProps(parent);

			    }).catch(function (error) {
			    	parent.isLoading = false;
			    	toastr.error('Error fetching records from server.', 'Error!');
			    });
		},

		_getQuery: function (id) {
			return this._datacontext.createQuery()
			    .from(this._entityInfo.resource)
			    .expand(this._entityInfo.expand)
			    .where("Id", "==", id);
		},

		_extendWithViewModelProps: function (parent) {

			var subItems = parent[this._entityInfo.childrenProperty];
			parent.hasChildren = subItems.length > 0;
			parent.isExpanded = parent.isExpanded && parent.hasChildren;

			for (var i = 0; i < subItems.length; ++i) {
				var subitem = parent[this._entityInfo.childrenProperty][i];
				subitem.hasChildren = subitem[this._entityInfo.childrenProperty].length > 0;
				subitem.isExpanded = !!subitem.isExpanded;
				subitem.isLoaded = subitem.isExpanded;
				subitem.isLoading = false;
			}
		},

		_rootLoaded: function (data) {

			var item = data.results[0];
			this._extendWithViewModelProps(item);

			if (item[this._entityInfo.parentProperty] == null) {
				this.$scope.item = item;
			} else {

				if (this._firstRootQuery) {
					this._selectItem(item);
					this._firstRootQuery = false;
				}

				item[this._entityInfo.parentProperty].isExpanded = true;
				this._datacontext.executeQuery(this._getQuery(item[this._entityInfo.parentPropertyId]))
				    .then(this._rootLoaded.bind(this))
				    .catch(this._rootNotLoaded.bind(this));
			}

			this.$scope.isLoadingRoot = false;
		},

		_rootNotLoaded: function (error) {
			toastr.error("Error fetching records from server.", "Error!");
			console.error(error);
		},

		_selectItem: function (item) {

			this._extendWithViewModelProps(item);

			if (item.entityAspect.entityState.name === 'Unchanged') {

				if (!item.isLoaded) {
					this._loadSubItems(item);
				}

				this.$scope.selectedItem = item;
				this.$location.path('/' + this._entityInfo.route + '/' + item.Id).replace();

				this._gridController.filters.clear();
				this._gridController.filters.add(this._entityInfo.childParentPropertyId, FilterCollection.booleanAnd.value, 'Equals', parseInt(item.Id));
				this._gridController._refresh();

			} else {
				toastr.warning('Save in order to access this item', 'Warning!');
			}
		},

		_setButtonVisibility: function () {

			var hasCreatePermission = this._permissions.hasPermission(this._entityInfo.entityType, 'Create');
			var hasDeletePermission = this._permissions.hasPermission(this._entityInfo.entityType, 'Delete');

			this._toolbar.createParentButton.visible = hasCreatePermission;
			this._toolbar.deleteParentButton.visible = hasDeletePermission;

			this._toolbar.linkChildButton.visible = this._isModal;
		},

		_updateButtonStates: function () {

			if (!this._gridController || !this._gridController.selection) return;

			var items = this._gridController.selection.getSelectedItems();
			var itemCount = items.length;

			if (this.$scope.isLoading) {
				_.each(this._toolbar, (function (button) { button.disabled = true; }));
			} else {

				this._toolbar.createParentButton.disabled = !this.$scope.selectedItem;
				this._toolbar.deleteParentButton.disabled = !this.$scope.selectedItem || this._entityInfo.hasChildren(this.$scope.selectedItem);
				//this._toolbar.deleteButton.disabled = !itemCount || _.some(items, function (item) {
				//	return !this._permissions.hasPermission(this._entityInfo.entityType, 'Modify', item.CreatorId);
				//}, this);

				this._toolbar.linkChildButton.disabled = (this._mode == 'one' && itemCount != 1) || (this._mode == 'many' && itemCount < 1);
				this._toolbar.saveButton.visible = this._toolbar.cancelButton.visible = this._datacontext.hasChanges();
			}
		},

		_createParent: function () {
			var item = this._datacontext.createEntity(this._entityInfo.entityType, this._settings.defaultEntityValues[this._entityInfo.entityType]);
			this.$scope.selectedItem[this._entityInfo.childrenProperty].push(item);
			this._extendWithViewModelProps(this.$scope.selectedItem);
			this.$scope.selectedItem.isExpanded = true;
			this._updateScope();
		},


		_deleteParent: function () {
			if (this.$scope.selectedItem) {
				this.$scope.selectedItem.entityAspect.setDeleted();
			}

			this._updateScope();
		},

		_saveEditedItem: function (item, event) {
			this._stopEditingItem(item);
			item.Name = this.$scope.temp.value;
			event && event.preventDefault();
		},

		_startEditingItem: function (item, event) {

			if (this._itemBeingEdited) {
				this._itemBeingEdited.editMode = false;
			}


			this._itemBeingEdited = item;
			this.$scope.temp = {
				value: item.Name
			};

			item.editMode = true;
			event && event.preventDefault();
		},

		_stopEditingItem: function (item) {
			this._itemBeingEdited = null;
			item.showEditOption = false;
			item.editMode = false;
		},


		// Drag and drop functions

		_onDropFiles: function (targetItem, files) {
			if (!this._isUploading) {
				_.each(files, (function (file) {
					this.$scope.files.push(file);
				}), this);

				this.$scope.files = _.uniq(this.$scope.files);
				this.$scope.$apply();
				this._uploadFiles(targetItem);
			} else {
				toastr.error('Wait until all file uploads have finished.', 'Error!');
			}
		},

		_onDrop: function (targetItem, transfer) {

			if (targetItem.entityAspect.entityState.name !== 'Unchanged') {
				toastr.warning('Save in order to access this item', 'Warning!');
				return;
			}

			var sourceType = transfer.data[0].type;
			var ids = _.pluck(transfer.data, "id");

			var that = this;

			if (sourceType == this._entityInfo.entityType && targetItem.id != transfer.data[0].id) {

				this._datacontext.fetchEntitiesByKey(this._entityInfo.resource, ids, function (data) {

					data.results.forEach(function (item) {
						var previousParent = item[that._entityInfo.parentProperty];
						_.defer(function () {
							that._extendWithViewModelProps(previousParent);
						});
						item[that._entityInfo.parentProperty] = targetItem;
						that._extendWithViewModelProps(item);
					});

					that._extendWithViewModelProps(targetItem);

				}, function (error) {
					toastr.error("Something went wrong! (" + error.message + ")", "Error!");
				}, that._entityInfo.expand);

			} else if (sourceType == that._entityInfo.childType) {

				that._datacontext.fetchEntitiesByKey(that._entityInfo.childResource, ids, function (data) {
					data.results.forEach(function (item) {
						item[that._entityInfo.childParent] = targetItem;
					});
				});
			} else {
				toastr.error("Cannot move this entity here.", "Error!");
			}
		},

		_onDragStart: function (item) {
			this._sourceItem = item;
		},

		_onDragEnd: function () {
			this._sourceItem = null;
		},

		_onDragEnter: function (item, event) {

			var that = this;

			this._canDrop = !this._sourceItem || !this._isItemChildOfSelf(item, this._sourceItem.Id);

			if (this._canDrop) {
				if (item.hasChildren) {
					this._delayedExpandItem = this.$timeout(function () {
						if (!item.isLoaded) {
							that._loadSubItems(item);
						} else {
							item.isExpanded = true;
						}
					}, 500);
				}
			}
		},

		_onDragLeave: function (item, event) {
			this.$timeout.cancel(this._delayedExpandItem);
		},

		_onDragOver: function (item, event) {
			event.dataTransfer.dropEffect = this._canDrop ? 'move' : 'none';
		},

		_isItemChildOfSelf: function (targetItem, sourceItemId) {

			if (targetItem.Id == sourceItemId) {
				return true;
			}

			while (targetItem.Parent != null) {
				targetItem = targetItem.Parent;
				if (targetItem.Id == sourceItemId) {
					return true;
				}
			}

			return false;
		},

		_uploadFiles: function (item) {

			$('#uploadFiles').modal('show');
			this._isUploading = true;

			_.each(this.$scope.files, (function (file) {
				var formData = new FormData();
				formData.append('folderId', item.Id);
				formData.append('file', file);

				var xhr = new XMLHttpRequest();
				xhr.upload.id = this._progressId;
				xhr.upload.addEventListener('progress', this._uploadProgress.bind(this), false);
				xhr.addEventListener('load', this._uploadComplete.bind(this), false);

				xhr.open('post', '/breeze/easydoc/upload/', true);
				xhr.send(formData);

				++this._progressId;
			}), this);

			this.$scope.$apply();
		},

		_uploadProgress: function (event) {
			this.$scope.files[this.id].state = "info";
			this.$scope.files[this.id].progress = Math.round(event.loaded * 100 / event.total);
			this.$scope.$apply();
		},

		_uploadComplete: function (event) {
			--this._progressId;

			switch (this.status) {
				case 300:
					toastr.error('There is already a file with this name in another folder on the server.', 'Error!');
					this.$scope.files[this.upload.id].state = "warning";
					break;
				case 302:
					toastr.error('There is already a file with this name in the folder.', 'Error!');
					this.$scope.files[this.upload.id].state = "warning";
					break;
				case 403:
					this.$scope.files[this.upload.id].state = "danger";
					break;
				case 404:
					toastr.error('Parent folder not found on server. It may have been deleted by another user. Refresh the webpage and try again.', 'Error!');
					this.$scope.files[this.upload.id].state = "danger";
					break;
				case 415:
					toastr.error('Unsupported upload request.', 'Error!');
					this.$scope.files[this.upload.id].state = "danger";
					break;
				case 500:
					toastr.error('Error uploading file.', 'Error!');
					this.$scope.files[this.upload.id].state = "danger";
					break;
				default:
					this.$scope.files[this.upload.id].state = "success";
					break;
			}

			if (this._progressId == 0) {
				this._isUploading = false;

				if (this.status == 403) {
					toastr.error('You do not have the required permissions to upload files.', 'Error!');
				} else if (this.status == 200) {
					toastr.success('All files have been uploaded.', 'Success!');
				}

				$('#uploadFiles').modal('hide');
				this.$scope.files = [];
				common.load();
			}
		}

	});

	angular.module('app').controller('TreeViewController', ['$rootScope', '$scope', '$state', '$location', '$timeout', 'Notifications', 'datacontext', 'modal', 'permissions', 'settings', TreeViewController]);

})();
