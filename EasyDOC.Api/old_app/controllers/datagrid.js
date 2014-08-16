(function () {

    angular.module('app').controller('datagrid', function ($rootScope, $scope, $state, datacontext, settings, modal, permissions, common, keyboard) {

        var isReady = false;

        var that = this;

        var id = parseInt($state.params.id);
        var type = $scope.config || $state.params.linkType || $state.params.type || $state.current.name;
        var mainType = $state.params.maintype;

        var mode = $state.params.mode;
        var isModal = !!mode;

        var config = settings.detailViewSettings[type];
        var columns = config.columns || (config[type] && config[type].columns) || (config[mainType] && config[mainType].columns);
        var entityInfo = _.findWhere(settings.queryParams, { route: type });

        var canDuplicate = !!entityInfo.duplicate;
        var filterFromState = !!mainType;

        // Filter away already selected items
        var hideLinkedEntities = $state.params.id && $state.params.filter && $state.params.filter.split('|');

        // Use the current location to build a unique key
        var uniqueKey = entityInfo.resource + (isModal ? '-select' : ('-' + window.location.pathname));
        var cacheLocally = uniqueKey && uniqueKey.length > 0;

        var paging = {
            current: 0,
            showStart: 0,
            showEnd: 0
        };

        var queryParams = {
            skip: 0,
            take: 30
        };

        var mainFilters = new FilterCollection({
            property: 'Id',
            type: 'number'
        });

        var hierarchyFilter = new FilterCollection({
            property: entityInfo.parentId,
            type: 'number'
        });


        if (columns && !_.findWhere(columns, { name: 'Created' }) && !config.skipAudit) {
            _.each(settings.detailViewSettings.auditDetails.columns, function (auditProperty) {
                columns.push(auditProperty);
            });
        }

        $scope.showToolbar = !config.hideToolbar;
        $scope.showStatusbar = !config.hideStatusbar;
        $scope.rowClass = config.rowClass;

        _.each(columns, (function (column, index) {
            column.order = index;
            column.visible = true;
            column.property = column.property || column.name;
            column.filterCollection = column.filterCollection || new FilterCollection(column);
        }));

        if (entityInfo.hierarchy && entityInfo.childRoute) {

            type = entityInfo.childRoute;
            entityInfo = _.findWhere(settings.queryParams, { route: type });

            if (!_.isNaN($state.params.id)) {
                hierarchyFilter.add(entityInfo.parentId, FilterCollection.booleanAnd.value, 'Equals', id);
            }
        }

        if (entityInfo.indirect) {
            _.extend(entityInfo, entityInfo.indirect[mainType]);
        }


        common.load = function () {
            that.executeQuery();
        };

        function getModelInfo(item, column) {
            var parts = column.property.split('.');
            parts.slice(0, -1).forEach(function (part) {
                item = item[part];
            });

            if (item == null)
                return null;

            return {
                model: item,
                value: item[parts.slice(-1)]
            };
        }

        function getRouteName(model) {
            return model.entityAspect._entityKey.entityType.shortName.toLowerCase();
        }

        function extendItemWithTemplate(item) {
            var templateConfig = {};
            _.each(columns, (function (col) {

                var info = getModelInfo(item, col);
                var route = info && getRouteName(info.model);

                templateConfig[col.name] = {
                    title: info ? info.value : '',
                    template: getTemplate(info, route, item, col),
                    permission: getPermission(info, route, item, col)
                };
            }));

            item._columns = templateConfig;
        }

        function getPermission(info, route, item, column) {
            return (column.basePermission || info) ? permissions.hasPermission(column.basePermission ? getRouteName(item) : route, 'Modify', column.basePermission ? item.CreatorId : info.model.CreatorId) : false;
        }

        function getTemplate(info, route, item, column) {

            var value = column.formatter ? '{{' + column.formatter + '(item.' + column.property + ', item)}}' : '{{item.' + column.property + '}}';

            if (column.url) {
                column.route = route;
                return '<span class="icn-indent ' + route + '"><a href="/' + route + '/{{item.' + column.url + '}}" title="' + value + '">' + value + '</a></span>';
            }

            return '<span title="{{item.' + column.property + '}}">' + value + '</span>';
        }

        var isEditing = false;

        $scope.isLoading = true;

        $scope.isEditing = function (item, column) {
            return item.editMode && column.editMode;
        };


        $scope.stopEditing = function (item, column) {
            isEditing = false;
            item.editMode = false;
            column.editMode = false;
        };

        $scope.startEditing = function (item, column, event) {

            isEditing = true;

            $scope.temp = {
                value: item[column.property]
            };
            item.editMode = true;
            column.editMode = true;

            if (column.type == 'enumeration') {

                var enumQuery = datacontext.createQuery().from(column.enumtype);
                datacontext.executeQuery(enumQuery)
                    .then(function (data) {
                        var array = [];
                        for (var i = 0; i < data.results.length; ++i) {
                            array.push({ Id: parseInt(i), Name: data.results[i] });
                        }
                        $scope.options = array;
                    });
            }

            event.preventDefault();
        };

        $scope.saveEditedItem = function (item, column, event) {
            $scope.stopEditing(item, column);
            item[column.property] = $scope.temp.value;

            if (event)
                event.preventDefault();
        };

        $scope.isEditable = true;

        $scope.paging = paging;
        $scope.queryParams = queryParams;

        $scope.$watch('queryParams.take', function () {
            if (!_.isNumber($scope.queryParams.take)) {
                $scope.queryParams.take = queryParams.take;
            } else if (datacontext.hasNewItems() && !config.local) {
                toastr.warning('Save new items before updating page', 'Warning!');
            } else if (isReady) {
                queryParams.take = $scope.queryParams.take;
                that.executeQuery();
            }
        });


        $scope.columnOrderOptions = {
            update: function () {
                resetHtmlElements();
            },
            axis: 'y'
        };

        $scope.selectAll = function (event) {
            if (event.target.checked) {
                selectable.selectAll();
            } else {
                selectable.deselectAll();
            }
        };

        $scope.$on('$stateChangeStart', function (event, toState, toParams, fromState) {
            if (datacontext.hasNewItems() || isModal) {
                if (!isModal) {
                    toastr.warning('Save new items before leaving page', 'Warning!');
                }
                event.preventDefault();
            }

            if (fromState.views.main && toState.views.main && fromState.views.main.controller == "browser" && toState.views.main.controller == "browser") {
                hierarchyFilter.clear();
                hierarchyFilter.add(entityInfo.parentId, FilterCollection.booleanAnd.value, 'Equals', parseInt(toParams.id));
                event.preventDefault();
                that.executeQuery();
            } else {
                selectable.deselectAll();
            }
        });

        $scope.hasNewItems = function () {
            return datacontext.hasNewItems();
        };

        function toggleSortDirection(column) {
            column.direction *= -1;
        }

        var sortedColumns = [];
        $scope.orderByProperty = function (property, event) {

            if (datacontext.hasNewItems() && !config.local) {
                toastr.warning('Save new items before reordering', 'Warning!');
                return;
            }

            var columnAlreadySorted = sortedColumns.some(function (item) {
                return item.property === property;
            });

            var hasChangedSorting = false;

            if (event.ctrlKey && sortedColumns.length) {
                var lastSorted = sortedColumns[sortedColumns.length - 1];

                if (lastSorted.property === property) {
                    toggleSortDirection(lastSorted);
                    if (lastSorted.direction == 0) {
                        sortedColumns.pop();
                    }
                    hasChangedSorting = true;
                } else if (!columnAlreadySorted) {
                    sortedColumns.push({ property: property, direction: 1 });
                    hasChangedSorting = true;
                }
            } else {
                if (sortedColumns.length == 1 && columnAlreadySorted) {
                    toggleSortDirection(sortedColumns[0]);
                    if (sortedColumns[0].direction == 0) {
                        sortedColumns.pop();
                    }
                } else {
                    sortedColumns = new Array({ property: property, direction: 1 });
                }
                hasChangedSorting = true;
            }

            for (var j in columns) {

                var unsorted = true;

                for (var i in sortedColumns) {
                    if (columns[j].property == sortedColumns[i].property) {
                        columns[j].sort = sortedColumns[i].direction;
                        unsorted = false;
                        break;
                    }
                }

                if (unsorted) {
                    columns[j].sort = 0;
                }
            }

            if (hasChangedSorting) {
                queryParams.skip = 0;
                that.executeQuery();
            }
        };

        $scope.requeryWithFilters = function () {
            if (datacontext.hasNewItems() && !config.local) {
                toastr.warning('Save new items before updating page', 'Warning!');
            } else {
                queryParams.skip = 0;
                that.executeQuery();
            }
        };

        $scope.booleanOperators = [FilterCollection.booleanAnd, FilterCollection.booleanOr];

        $scope.selectedColumn = null;
        $scope.showFilterBuilder = function (column) {
            $scope.selectedColumn = column;
        };

        $scope.gotoPage = function (page) {
            if (datacontext.hasNewItems() && !config.local) {
                toastr.warning('Save new items before updating page', 'Warning!');
            } else {
                queryParams.skip = queryParams.take * (page - 1);
                that.executeQuery();
            }
        };

        $scope.addFilter = function (column) {
            column.filterCollection.addDefault();
        };

        $scope.canAddFilter = function (column) {
            return column.filterCollection.canAdd();
        };

        $scope.$on('load', function () {
            isReady = true;
            that.executeQuery();
        });

        $scope.$on('refreshList', function () {
            _.each(columns, (function (column) { column.filterCollection.clear(); }));
            isReady = true;
            that.executeQuery();
        });

        $scope.toggleColumn = function (column) {
            column.visible = !column.visible;
            resetHtmlElements();
            storeSettingsLocally();
        };

        $scope.link = function (item, column, event) {

            event.preventDefault();

            if (window.opener) {
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

            modal.openSelectListWindow(routeName, false, getSelectListFilter(), function (data) {

                var selectedId = parseInt(data.ids[0]);
                var resource = settings.getResource(actualRoute);

                datacontext.fetchEntitiesByKey(resource, [selectedId], function (result) {
                    setPropertyOfSelectedItems(item, column, result.results[0]);
                });
            });
        };

        $scope.unlink = function (item, column) {
            setPropertyOfSelectedItems(item, column, null);
        };

        $scope.getFormattedSize = common.getFormattedSize;
        $scope.getUserInitials = common.getUserInitials;
        $scope.getGanttInfo = common.getGanttInfo;
        $scope.getGanttLevel = common.getGanttLevel;

        function setPropertyOfSelectedItems(item, column, value) {

            _.each(_.union(item, getSelectedItems()), (function (selectedItem) {

                var path = column.linkProperty.split('.');
                var currentItem = selectedItem;

                path.slice(0, -1).forEach(function (property) {
                    currentItem = currentItem[property];
                });

                currentItem[path.slice(-1)] = value;
            }));
        }

        function getSelectListFilter() {
            return [entityInfo.resource, entityInfo.fromId, entityInfo.toId, $state.params.id].join('|');
        }

        function generateQuery() {

            var order = entityInfo.sort || 'Name';

            if (sortedColumns.length) {
                order = '';
                for (var i = 0; i < sortedColumns.length; ++i) {
                    order += i > 0 ? ',' : '';
                    order += sortedColumns[i].property;
                    order += sortedColumns[i].direction < 0 ? ' desc' : '';
                }
            } else {
                sortedColumns[0] = { property: entityInfo.sort || 'Name', direction: 1 };
            }

            var query = datacontext.createQuery()
                .from(entityInfo.resource)
                .orderBy(order)
                .expand(entityInfo.expand)
                .skip(queryParams.skip)
                .take(queryParams.take)
                .inlineCount();

            var predicate = hierarchyFilter.apply();
            predicate = mainFilters.apply(predicate);

            _.each(columns, function (column) {
                predicate = column.filterCollection.apply(predicate);
            });

            return query.where(predicate);
        }

        function clearSelection() {
            selectable.deselectAll();
            $scope.$emit('resetSelectedItems');
        }

        var remoteQueryExecuted = config.remote;

        this.executeQuery = function () {

            setBusy(true);
            storeSettingsLocally();
            clearSelection();

            if (config.local && remoteQueryExecuted) {
                entitiesLoaded({
                    results: datacontext.executeQueryLocally(generateQuery())
                });
            } else {
                datacontext.executeQuery(generateQuery(), entitiesLoaded, errorLoading);
            }
        };

        $scope.columns = columns;

        $scope.$on('loaded', function () {
            if (config.remote) {
                entitiesLoaded({
                    results: $scope.items
                });
            }
        });

        function setBusy(value) {
            $scope.isLoading = value;
        }

        function entitiesLoaded(data) {

            remoteQueryExecuted = true;
            $scope.count = data.inlineCount;
            $scope.items = data.results;

            $scope.items.forEach(function (item) {
                extendItemWithTemplate(item);
            });

            paging.current = Math.floor(queryParams.skip / queryParams.take) + 1;
            paging.showStart = queryParams.skip + 1;
            paging.showEnd = Math.min(queryParams.skip + queryParams.take, data.inlineCount);

            // HACK: this is placed here in order for the 
            // dropdown menu not to close when clicked. Needs
            // to be run after the html has finished loading.
            resetHtmlElements();

            setBusy(false);
        }

        function resetHtmlElements() {

            /*$("#table-grid").colResizable({
                liveDrag: true,
                minWidth: 30,
                draggingClass: ''
            });*/

        }

        function errorLoading(error) {
            setBusy(false);
            toastr.error('Error fetching records from server.', 'Error!');
            console.error(error);
        }

        function storeSettingsLocally() {
            if (cacheLocally && columns.length) {
                var toCache = {
                    filters: _.pluck(columns, 'filters'),
                    query: queryParams,
                    order: sortedColumns,
                    visibility: _.pluck(columns, 'visible')
                };
                localStorage.setItem(uniqueKey, JSON.stringify(toCache));
            }
        }

        function loadColumnFilterEnums(column, enumType) {
            var enumQuery = datacontext.createQuery().from(enumType);
            datacontext.executeQuery(enumQuery, function (data) {
                column.enums = data.results.sort();
            });
        }

        function loadSettingsFromCache() {
            var fromCache = JSON.parse(localStorage.getItem(uniqueKey));
            if (fromCache) {

                _.each(fromCache.filters, function (filters, index) {
                    _.each(filters, function (filter) {
                        if (filter) {
                            columns[index].filters.push(filter);
                        }
                    });
                });

                _.each(fromCache.visibility, function (visible, index) {
                    columns[index].visible = visible;
                });

                queryParams = fromCache.query || queryParams;
                sortedColumns = fromCache.order || sortedColumns;

                _.each(columns, function (column) {
                    if (column.type == 'enumeration') {
                        loadColumnFilterEnums(columns[column.order], column.enumtype);
                    }
                });
            }

            $scope.queryParams = queryParams;
        }

        var toolbar = {
            createButton: { order: 0, action: createNewItem, icon: entityInfo.multiKey ? 'fa-link' : 'fa-file-o', tooltip: entityInfo.multiKey ? 'Create new relation' : 'Create new entity', visible: false },
            copyButton: { order: 1, action: copyItem, icon: 'fa-copy', tooltip: 'Copy selected entities', visible: false },
            deleteButton: { order: 2, action: deleteItems, icon: 'fa-trash-o', tooltip: 'Delete selected entities', visible: false },
            resetButton: { order: 3, action: resetItems, icon: 'fa-retweet', tooltip: 'Reset selected entities', visible: false },
            refreshButton: { order: 4, action: refreshList, icon: 'fa-refresh', tooltip: 'Refresh content', visible: true },
            linkButton: { order: 5, action: linkItem, icon: 'fa-link', tooltip: 'Link selected entities', visible: false },
            importButton: { order: 6, action: importItem, icon: 'fa-cloud-upload', tooltip: 'Import from Excel', visible: false },
            exportButton: { order: 7, action: exportItem, icon: 'fa-cloud-download', tooltip: 'Export to Excel', visible: true }
        };

        $scope.toolbar = _.toArray(toolbar);
        $scope.isEditable = false;

        permissions.whenReady(function () {
            toolbar.createButton.visible = permissions.hasPermission(entityInfo.entityType, 'Create');
            toolbar.importButton.visible = permissions.hasPermission(entityInfo.entityType, 'Create');
            toolbar.copyButton.visible = permissions.hasPermission(entityInfo.entityType, 'Create');
            toolbar.deleteButton.visible = permissions.hasPermission(entityInfo.entityType, 'Delete');

            $scope.isEditable = permissions.hasPermission(entityInfo.entityType, 'Modify');
        });

        $rootScope.$watch(updateButtonStates);

        function updateButtonStates() {
            var items = getSelectedItems();
            var itemCount = items.length;

            toolbar.linkButton.visible = isModal;

            if ($scope.isLoading) {
                _.each(toolbar, (function (button) { button.disabled = true; }));
            } else {
                toolbar.linkButton.disabled = (mode == 'one' && itemCount != 1) || (mode == 'many' && itemCount < 1);
                toolbar.createButton.disabled = itemCount;
                toolbar.copyButton.disabled = !(canDuplicate && itemCount == 1);
                toolbar.deleteButton.disabled = !itemCount || items.some(function (item) {
                    return !permissions.hasPermission(entityInfo.entityType, 'Modify', item.CreatorId);
                });
                toolbar.resetButton.disabled = !itemCount;
                toolbar.importButton.disabled = itemCount;
                toolbar.exportButton.disabled = false;
                toolbar.refreshButton.disabled = datacontext.hasNewItems();
            }
        }

        var selectable = new Selectable($scope);

        function keyboardHandler(event) {
            switch (event.keyCode) {

                case 37:
                    // Left arrow
                    break;

                case 38:
                    // Up arrow
                    if (!isEditing && $scope.cursor > 0) {
                        $scope.selectItem($scope.items[$scope.cursor - 1], event, $scope.cursor - 1);
                        event.preventDefault();
                    }
                    break;

                case 39:
                    // Right arrow

                    break;

                case 40:
                    // Down arrow
                    if (!isEditing && $scope.cursor < $scope.items.length - 1) {
                        $scope.selectItem($scope.items[$scope.cursor + 1], event, $scope.cursor + 1);
                        event.preventDefault();
                    }
                    break;
            }

            if (!$scope.$$phase)
                $scope.$apply();
        }

        $scope.selectItem = function (item, event, index) {
            var cursor = selectable.selectItem(item, event, index, $scope.cursor);
            if (cursor != null) {
                $scope.cursor = cursor;
            }

            keyboard.setHandler(keyboardHandler);
            $scope.$emit('selectionChanged', selectable);
        };

        function getSelectedItems() {
            var selectedItems = selectable.getSelectedItems();
            $scope.selectedItemCount = selectedItems.length;
            return selectedItems;
        }

        $scope.getSelectedItems = getSelectedItems;

        function linkItem() {
            if (isModal) {
                if ($state.params.CKEditorFuncNum) {
                    var file = getSelectedItems()[0];
                    window.opener.CKEDITOR.tools.callFunction($state.params.CKEditorFuncNum, $scope.getUrl(file));
                    window.close();
                } else {
                    if (datacontext.hasNewItems()) {
                        toastr.warning('Save item before linking.', 'Warning!');
                        return;
                    }
                    modal.postMessageAndCloseWindow({ type: type, ids: _.pluck(getSelectedItems(), 'Id') });
                }
            }
        }

        $scope.getUrl = function (file) {
            if (file != null) {
                var url = '/' + file.Name + '.' + file.Type;
                var parent = file.Folder;

                while (parent != null) {
                    url = '/' + parent.Name + url;
                    parent = parent.Parent;
                }

                return url;
            }

            return null;
        };

        function createNewItem() {

            var setCustomProperties = function (createdEntity, entity) {
                if (entityInfo.to == 'file') {
                    createdEntity.Name = entity.Name;
                }
            };

            var entityType = entityInfo.entityType;

            // Multikey entities should be selected from a list in a modal window and not created directly
            if (entityInfo.multiKey) {

                modal.openSelectListWindow(entityInfo.toRoute || entityInfo.to, true, getSelectListFilter(), function (selection) {

                    if (selection.ids.length > 0) {

                        datacontext.fetchEntitiesByKey(entityInfo.toResource, selection.ids, function (data) {

                            var newEntities = [];

                            data.results.forEach(function (entity) {

                                var defaultValues = settings.getDefaultValuesForEntity(entityInfo.entityType);
                                defaultValues[entityInfo.fromId] = $state.params.id;
                                defaultValues[entityInfo.toProperty] = entity;

                                if (setCustomProperties) {
                                    setCustomProperties(defaultValues, entity);
                                }

                                try {
                                    var prepareEntity = datacontext.createEntity(entityType, defaultValues);
                                    extendItemWithTemplate(prepareEntity);
                                    newEntities.push(prepareEntity);
                                } catch (error) {
                                    toastr.error('Item already exists.', 'Error!');
                                    console.log(error);
                                }
                            });

                            $scope.items = _.union(newEntities, $scope.items);

                        }, function (error) {
                            toastr.error('Error fetching records from server.', 'Error!');
                            console.log(error);
                        }, entityInfo.toExpand);
                    }
                });
            } else {
                var values = settings.getDefaultValuesForEntity(entityInfo.entityType);

                if (entityInfo.fromId) {
                    values[entityInfo.fromId] = $state.params.id;
                }

                values['CreatorId'] = permissions.getCurrentUserId();

                var newEntity = datacontext.createEntity(entityType, values);
                extendItemWithTemplate(newEntity);
                $scope.items = _.union([newEntity], $scope.items);
            }
        }

        function copyItem() {

            setBusy(true);

            var originalEntity = getSelectedItems()[0];
            var copy = {};

            entityInfo.duplicate.split(',').forEach(function (property) {
                copy[property] = originalEntity[property];
            });

            copy['CreatorId'] = permissions.getCurrentUserId();
            copy['Name'] = copy['Name'] + ' (copy)';
            copy['NameSe'] = copy['NameSe'] + ' (copy)';

            var expand = _.pluck(entityInfo.duplicateExpand, 'name').join();

            // Query the server so that all entities are in the local cache
            var query = datacontext.createQuery()
                .from(entityInfo.resource)
                .where('Id', '==', originalEntity.Id)
                .expand(expand);

            datacontext.executeQuery(query, function () {

                var newEntity = datacontext.createEntity(entityInfo.entityType, copy);
                extendItemWithTemplate(newEntity);

                // Iterate over all the navigation property collections that should be duplicated
                expand.split(',').forEach(function (collection) {

                    // Extract an array containing all the properties that should be copied from the original entity to the new entity
                    var entityProperties = _.findWhere(entityInfo.duplicateExpand, { name: collection }).properties.split(',');

                    // Iterate over all the instances within a collection
                    originalEntity[collection].forEach(function (relation) {

                        // Create a default new entity and copy the properties from the original entity
                        var entityType = relation.entityAspect._entityKey.entityType.shortName;
                        var defaultValues = settings.getDefaultValuesForEntity(entityType);
                        entityProperties.forEach(function (property) {
                            defaultValues[property] = relation[property];
                        });

                        // Relate this entity to the new entity
                        defaultValues[entityInfo.duplicateId] = newEntity.Id;

                        // Create the entity in the datacontext
                        datacontext.createEntity(entityType, defaultValues);
                    });
                });

                $scope.items = _.union([newEntity], $scope.items);
                setBusy(false);

            }, function (error) {
                toastr.error(error.message, "Error!");
                console.error(error);
                setBusy(false);
            });
        }


        function importItem() {
            if (window.opener == null) {
                //modal.openImportWindow(function (data) {
                //    data.content.split('\n').forEach(function (line) {
                //        var cols = line.split('\t');
                //    });
                //});
            } else {
                toastr.warning('Can\'t open a second popup window.', 'Warning!');
            }
        }


        function exportItem() {
        }


        function deleteItems() {
            getSelectedItems().forEach(function (item) {
                if (item.entityAspect.entityState.name !== 'Detached') {
                    item.entityAspect.setDeleted();
                    item.isSelected = false;
                }
            });
        }

        function resetItems() {
            getSelectedItems().forEach(function (item) {
                if (item.entityAspect.entityState.name !== 'Detached') {
                    item.entityAspect.rejectChanges();
                }
            });
        }

        function refreshList() {
            $scope.$broadcast('refreshList');
        }

        function loadGrid() {
            if (filterFromState) {
                mainFilters.add(entityInfo.fromId, FilterCollection.booleanAnd.value, 'Equals', $state.params.id);
                that.executeQuery();
            } else if (config.filter) {
                mainFilters.add(config.filter, FilterCollection.booleanAnd.value, 'Equals', $state.params.id);
                that.executeQuery();
            } else if (entityInfo.load) {
                isReady = true;
                that.executeQuery();
            }
        }

        if (config.remote) {
            $scope.$watch('items', function() {
                _.each($scope.items, function(item) {
                    if (!item._columns) {
                        extendItemWithTemplate(item);
                    }
                });
            });
        }

        loadSettingsFromCache();

        permissions.whenReady(function () {

            if (hideLinkedEntities) {

                var query = datacontext.createQuery()
                    .from(filter[0])
                    .where(filter[1], '==', filter[3])
                    .select(filter[2]);

                datacontext.executeQuery(query, function (data) {
                    data.results.forEach(function (item) {
                        mainFilters.add('Id', FilterCollection.booleanAnd.value, 'NotEquals', item[filter[2]]);
                    });

                    loadGrid();
                });

            } else {
                loadGrid();
            }
        });
    });
})();