(function() {

    var CompleteGridController = CommonGridController.extend({

        /**
	 * Initialize Notes Controller
	 * @param $scope, current controller scope
	 */
        init: function($rootScope, $scope, $state, Notifications, datacontext, modal, permissions, settings) {

            var that = this;

            this._parentId = parseInt($state.params.id);
            var type = $scope.config || $state.params.linkType || $state.params.type || $state.current.name;
            var mainType = $state.params.maintype;
            var filterOnParentId = !!mainType;

            var config = settings.detailViewSettings[type];
            var columns = config.columns || (config[type] && config[type].columns) || (config[mainType] && config[mainType].columns);

            this._entityInfo = _.findWhere(settings.queryParams, { route: type });
            this._mode = $state.params.mode;
            this._isModal = !!this._mode;
            this._type = type;
            this._uniqueKey = this._entityInfo.resource + (this._isModal ? '-select' : '');

            if (this._entityInfo.indirect) {
                _.extend(this._entityInfo, this._entityInfo.indirect[mainType]);
            }

            // Extend columns with audit information fields as defined in the settings
            if (!_.findWhere(columns, { name: 'Opprettet' }) && !config.skipAudit) {
                _.each(settings.detailViewSettings.auditDetails.columns, function(auditProperty) {
                    columns.push(auditProperty);
                }, this);
            }

            $scope.$on('gridReady', function(event, gridController) {
                that._gridController = gridController;
                that._loadSettings(columns);

                if (filterOnParentId) {
                    that._gridController.filters.add(that._entityInfo.fromId, FilterCollection.booleanAnd.value, 'Equals', that._parentId);
                }

                that._gridController._setConfiguration([that._parentId, config, columns, that._entityInfo, that]);
                that._updateButtonStates();
            });

            $rootScope.$watch(this._updateButtonStates.bind(this));

            this._toolbar = {
                copyButton: { order: 1, action: this._copyItem.bind(this), icon: 'fa-copy', tooltip: { title: 'Lag kopi' }, visible: false },
                deleteButton: { order: 2, action: this._deleteItems.bind(this), icon: 'fa-trash-o', tooltip: { title: 'Slett' }, visible: false },
                //revertButton: { order: 3, action: this._revertItems.bind(this), icon: 'fa-retweet', tooltip: { title: 'Tilbakestill' }, visible: false },
                resetButton: { order: 4, action: this._resetGrid.bind(this), icon: 'fa-refresh', tooltip: { title: 'Oppdater' }, visible: true },
                linkButton: { order: 5, action: this._linkToParent.bind(this), icon: 'fa-link', tooltip: { title: 'Link' }, visible: false },
                importButton: { order: 6, action: this._importItem.bind(this), icon: 'fa-cloud-upload', tooltip: { title: 'Lim inn fra Excel' }, visible: false },
                exportButton: { order: 7, action: this._exportItem.bind(this), icon: 'fa-cloud-download', tooltip: { title: 'Kopier til Excel' }, visible: false },
                saveButton: { order: 8, action: this._saveChanges.bind(this), icon: 'fa-save', tooltip: { title: 'Lagre' }, visible: false },
                cancelButton: { order: 9, action: this._cancelChanges.bind(this), icon: 'fa-rotate-left', tooltip: { title: 'Avbryt' }, visible: false }
            };

            if (this._entityInfo.multiKey) {
                this._toolbar.createButton = { order: 0, action: this._create.bind(this), icon: 'fa-link', tooltip: { title: 'Ny' }, visible: false };
            } else {
                this._toolbar.createButton = { order: 0, action: this._create.bind(this), icon: 'fa-file', tooltip: { title: 'Ny' }, visible: false };
            }

            permissions.whenReady(this._setButtonVisibility.bind(this));

            this._super($rootScope, $scope, $state, Notifications, datacontext, modal, permissions, settings);
        },

        /**
	 *@Override
	 */
        defineListeners: function() {
            this._listeners = {
                'ui.navigation.events.NEW': this._create.bind(this),
                'ui.navigation.events.DELETE': this._deleteItems.bind(this)
            };

            this._super();
        },


        defineScope: function() {

            this.$scope.toggleFilters = this._toggleFilters.bind(this);
            this._super();
        },


        destroy: function() {
            this._gridController.selection.deselectAll();
            this._super();
        },


        _setButtonVisibility: function() {

            var hasCreatePermission = this._permissions.hasPermission(this._entityInfo.entityType, 'Create');
            var hasModifyPermission = this._permissions.hasPermission(this._entityInfo.entityType, 'Modify');
            var hasDeletePermission = this._permissions.hasPermission(this._entityInfo.entityType, 'Delete');

            this._toolbar.createButton.visible = hasCreatePermission;
            this._toolbar.importButton.visible = false; //hasCreatePermission;
            this._toolbar.exportButton.visible = false; //hasCreatePermission;
            this._toolbar.copyButton.visible = hasCreatePermission;
            //this._toolbar.revertButton.visible = hasModifyPermission;
            this._toolbar.deleteButton.visible = hasDeletePermission;

            this._toolbar.linkButton.visible = this._isModal;
        },

        _updateButtonStates: function() {

            if (!this._gridController || !this._gridController.selection) return;

            var items = this._gridController.selection.getSelectedItems();
            var itemCount = items.length;

            if (this.$scope.isLoading) {
                _.each(this._toolbar, (function(button) { button.disabled = true; }));
            } else {

                var canDuplicate = !!this._entityInfo.duplicate;

                this._toolbar.linkButton.disabled = (this._mode == 'one' && itemCount != 1) || (this._mode == 'many' && itemCount < 1);
                this._toolbar.createButton.disabled = itemCount;
                this._toolbar.copyButton.disabled = !(canDuplicate && itemCount == 1);
                //this._toolbar.revertButton.disabled = !itemCount;
                this._toolbar.importButton.disabled = itemCount;
                this._toolbar.exportButton.disabled = false;
                this._toolbar.resetButton.disabled = this._datacontext.hasNewItems();

                this._toolbar.saveButton.visible = this._toolbar.cancelButton.visible = this._datacontext.hasChanges();

                this._toolbar.deleteButton.disabled = !itemCount || _.some(items, function(item) {
                    return !this._permissions.hasPermission(this._entityInfo.entityType, 'Modify', item.CreatorId);
                }, this);
            }
        },

        _create: function() {
            if (this._entityInfo.multiKey) {
                this._createDefaultMultiKeyEntity();
            } else {
                this._createDefaultEntity();
            }

            this._updateScope();
        },

        _createDefaultEntity: function() {

            var values = this._settings.getDefaultValuesForEntity(this._entityInfo.entityType);

            if (this._entityInfo.fromId) {
                values[this._entityInfo.fromId] = this._parentId;
            }

            values['CreatorId'] = this._permissions.getCurrentUserId();

            var newEntity = this._datacontext.createEntity(this._entityInfo.entityType, values);
            this._gridController.addItem(newEntity);
        },

        _createDefaultMultiKeyEntity: function() {

            var that = this;

            this._modal.openSelectListWindow(this._entityInfo.toRoute || this._entityInfo.to, true, this._getLinkedItemsFilter(), function(selection) {

                if (selection.ids.length > 0) {

                    that._datacontext.fetchEntitiesByKey(that._entityInfo.toResource, selection.ids, function(data) {

                        data.results.forEach(function(entity) {

                            var defaultValues = that._settings.getDefaultValuesForEntity(that._entityInfo.entityType);
                            defaultValues[that._entityInfo.fromId] = that._parentId;
                            defaultValues[that._entityInfo.toProperty] = entity;

                            //setCustomProperties(defaultValues, entity);

                            try {
                                var newEntity = that._datacontext.createEntity(that._entityInfo.entityType, defaultValues);
                                that._gridController.addItem(newEntity);
                            } catch (error) {
                                toastr.error('Item already exists.', 'Error!');
                                console.log(error);
                            }
                        });

                    }, function(error) {
                        toastr.error('Error fetching records from server.', 'Error!');
                        console.log(error);
                    }, that._entityInfo.toExpand);
                }
            });
        },

        _copyItem: function() {

            var originalEntity = this._gridController.selection.getSelectedItems()[0];
            var copy = {};

            this._entityInfo.duplicate.split(',').forEach(function(property) {
                copy[property] = originalEntity[property];
            });

            copy['CreatorId'] = this._permissions.getCurrentUserId();
            copy['Name'] = copy['Name'] + ' (copy)';
            copy['NameSe'] = copy['NameSe'] + ' (copy)';

            var expand = _.pluck(this._entityInfo.duplicateExpand, 'name').join();

            // Query the server so that all entities are in the local cache
            var query = this._datacontext.createQuery()
                .from(this._entityInfo.resource)
                .where('Id', '==', originalEntity.Id)
                .expand(expand);

            var that = this;

            this._datacontext.executeQuery(query)
                .then(function() {

                    var newEntity = that._datacontext.createEntity(that._entityInfo.entityType, copy);
                    that._gridController.addItem(newEntity);

                    // Iterate over all the navigation property collections that should be duplicated
                    expand.split(',').forEach(function(collection) {

                        // Extract an array containing all the properties that should be copied from the original entity to the new entity
                        var entityProperties = _.findWhere(that._entityInfo.duplicateExpand, { name: collection }).properties.split(',');

                        // Iterate over all the instances within a collection
                        originalEntity[collection].forEach(function(relation) {

                            // Create a default new entity and copy the properties from the original entity
                            var entityType = relation.entityAspect._entityKey.entityType.shortName;
                            var defaultValues = that._settings.getDefaultValuesForEntity(entityType);
                            entityProperties.forEach(function(property) {
                                defaultValues[property] = relation[property];
                            });

                            // Relate this entity to the new entity
                            defaultValues[that._entityInfo.duplicateId] = newEntity.Id;

                            // Create the entity in the datacontext
                            that._datacontext.createEntity(entityType, defaultValues);
                        });
                    });

                }).catch(function(error) {
                    toastr.error(error.message, "Error!");
                    console.error(error);
                });
        },

        _importItem: function() {
            if (window.opener == null) {
                //modal.openImportWindow(function (data) {
                //    data.content.split('\n').forEach(function (line) {
                //        var cols = line.split('\t');
                //    });
                //});
            } else {
                toastr.warning('Can\'t open a second popup window.', 'Warning!');
            }
        },


        _exportItem: function() {
        },

        _deleteItems: function() {
            var items = this._gridController.selection.getSelectedItems();
            items.forEach(function(item) {
                if (item.entityAspect.entityState.name !== 'Detached') {
                    item.entityAspect.setDeleted();
                }
            });

            this._updateScope();
        },

        _toggleFilters: function() {
            this._gridController._toggleFilters();
        },

        _revertItems: function() {
            var items = this._gridController.selection.getSelectedItems();
            items.forEach(function(item) {
                if (item.entityAspect.entityState.name !== 'Detached') {
                    item.entityAspect.rejectChanges();
                }
            });
        },

        _resetGrid: function() {
            this._gridController.reset();
        },

        openModalLinkWindow: function(item, column) {

            if (this._isModal) {
                toastr.warning('Can\'t open a second popup window.', 'Warning!');
                return;
            }

            var routeName = column.link;
            var actualRoute = column.link;

            if (routeName == 'folder' || routeName == 'category') {
                routeName += '/1';
                if (column.link == 'folder') {
                    actualRoute = 'file';
                }
            }

            var that = this;

            this._modal.openSelectListWindow(routeName, false, this._getLinkedItemsFilter(), function(data) {

                var selectedId = parseInt(data.ids[0]);
                var resource = that._settings.getResource(actualRoute);

                that._datacontext.fetchEntitiesByKey(resource, [selectedId], function(result) {
                    that._gridController._setPropertyOfSelectedItems(item, column, result.results[0]);
                });
            });
        }

    });

    angular.module('app').controller('CompleteGridController', ['$rootScope', '$scope', '$state', 'Notifications', 'datacontext', 'modal', 'permissions', 'settings', CompleteGridController]);

})();