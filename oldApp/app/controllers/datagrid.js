(function () {

    angular.module('app').controller('datagrid', function ($rootScope, $scope, $state, $location, datacontext, settings, modal, clipboard, permissions, common, signalr) {

        $scope.isLoading = true;

        var self = this;
        var isReady = false;

        var filtersFromCache = [];
        var visibilityFromCache = [];
        var type = $state.params.linkType || $state.params.type || $state.current.name;

        var mode = $state.params.mode;
        var isModal = !!mode;

        var entityInfo = _.findWhere(settings.queryParams, { route: type });
        if (entityInfo.hierarchy && entityInfo.childRoute) {
            type = entityInfo.childRoute;
            entityInfo = _.findWhere(settings.queryParams, { route: type });
        }

        if (entityInfo.indirect) {
            _.extend(entityInfo, entityInfo.indirect[$state.params.maintype]);
        }

        var filterFromState = !!$state.params.maintype;

        // Filter away already selected items
        var filter = $state.params.filter;
        filter = $state.params.id && filter && filter.split('|');

        // Use the current location to build a unique key
        var uniqueKey = entityInfo.resource + (mode ? '-select' : ('-' + window.location.pathname));
        var cacheLocally = uniqueKey && uniqueKey.length > 0;
        var defaultTake = 25;

        var booleanAnd = { value: 1, name: 'And' };
        var booleanOr = { value: 2, name: 'Or' };

        var filters = [];

        var paging = {
            current: 0,
            showStart: 0,
            showEnd: 0
        };

        var queryParams = {
            skip: 0,
            take: defaultTake
        };

        common.load = function () {
            self.executeQuery();
        };

        $scope.paging = paging;
        $scope.queryParams = queryParams;

        $scope.$watch('queryParams.take', function () {
            if (datacontext.hasNewItems()) {
                toastr.warning('Save new items before updating page', 'Warning!');
            } else if (isReady) {
                queryParams.take = $scope.queryParams.take;
                self.executeQuery();
            }
        });

        $scope.columnOrderOptions = {
            update: function (event, ui) {
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

        $scope.$on('$stateChangeStart', function (event, toState, toParams, fromState, fromParams) {
            if (datacontext.hasNewItems() || isModal) {
                if (!isModal) {
                    toastr.warning('Save new items before leaving page', 'Warning!');
                }
                event.preventDefault();
                $location.path($state.href(fromState, fromParams));
            }

            if (fromState.views.main && toState.views.main && fromState.views.main.controller == "browser" && toState.views.main.controller == "browser") {
                $state.params.id = parseInt(toParams.id);
                event.preventDefault();
                self.executeQuery();
            }
        });

        $scope.hasNewItems = function () {
            return datacontext.hasNewItems();
        };

        function toggleSortDirection(column) {
            column.direction *= -1;
            return column;
        }

        var sortedColumns = [];
        $scope.orderByProperty = function (property, event) {

            if (datacontext.hasNewItems()) {
                toastr.warning('Save new items before reordering', 'Warning!');
                return;
            }

            var columnAlreadySorted = _.some(sortedColumns, function (item) {
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

                for (i in sortedColumns) {
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
                self.executeQuery();
            }
        };

        $scope.requeryWithFilters = function () {
            if (datacontext.hasNewItems()) {
                toastr.warning('Save new items before updating page', 'Warning!');
            } else {
                queryParams.skip = 0;
                self.executeQuery();
            }
        };

        $scope.modifyDisabled = function (type, id) {
            return false;
            // TODO: optimize this for performance. Needs memoizing.
            //return !permissions.hasPermission(type, 'Modify', id);
        };

        $scope.queryOperators = {
            string: [
					{ id: 0, op: 'Contains', negate: false, name: 'Contains' },
					{ id: 1, op: 'Contains', negate: true, name: 'Not Contains' },
					{ id: 2, op: 'EndsWith', negate: false, name: 'Ends With' },
					{ id: 3, op: 'EndsWith', negate: true, name: 'Not Ends With' },
					{ id: 4, op: 'Equals', negate: false, name: 'Equals' },
					{ id: 5, op: 'NotEquals', negate: false, name: 'Not Equals' },
					{ id: 6, op: 'StartsWith', negate: false, name: 'Starts With' },
					{ id: 7, op: 'StartsWith', negate: true, name: 'Not Starts With' }
            ],
            number: [
					{ id: 0, op: 'Equals', negate: false, name: '=' },
					{ id: 1, op: 'NotEquals', negate: false, name: '<>' },
					{ id: 1, op: 'GreaterThan', negate: false, name: '>' },
					{ id: 2, op: 'GreaterThanOrEqual', negate: false, name: '>=' },
					{ id: 3, op: 'LessThan', negate: false, name: '<' },
					{ id: 4, op: 'LessThanOrEqual', negate: false, name: '<=' }
            ],
            enumeration: [
					{ id: 0, op: 'Equals', negate: false, name: 'Equals' },
					{ id: 1, op: 'NotEquals', negate: true, name: 'Not Equals' }
            ],
            bool: [
					{ id: 0, op: 'Equals', negate: false, name: 'Is' }
            ]
        };

        $scope.booleanOperators = [booleanAnd, booleanOr];

        $scope.selectedColumn = null;
        $scope.showFilterBuilder = function (column) {
            $scope.selectedColumn = column;
        };

        $scope.gotoPage = function (page) {
            if (datacontext.hasNewItems()) {
                toastr.warning('Save new items before updating page', 'Warning!');
            } else {
                queryParams.skip = queryParams.take * (page - 1);
                self.executeQuery();
            }
        };

        $scope.addFilter = function (column) {
            if ($scope.canAddFilter(column.filters)) {
                column.filters.push({
                    bool: booleanAnd.value,
                    op: $scope.queryOperators[column.type][0].id,
                    value: column.type == 'enumeration' ? column.enums[0] : ''
                });
            }
        };

        $scope.canAddFilter = function (columnFilters) {
            var length = columnFilters.length;
            return length == 0 || columnFilters[length - 1].value;
        };

        $scope.$on('load', function (event) {
            isReady = true;
            self.executeQuery();
        });

        $scope.$on('refreshList', function (event) {
            columns.forEach(function (column) { column.filters = []; });
            isReady = true;
            self.executeQuery();
        });

        $scope.linkItem = function () {
            linkItem();
        };

        $scope.toggleColumn = function (column) {
            column.visible = !column.visible;
            resetHtmlElements();
            saveColumnsInCache();
        };

        $scope.select = function (item, column) {

            if (window.opener == null) {

                var routeName = column.link;
                var actualRoute = column.link;

                if (routeName == 'folder' || routeName == 'category') {
                    routeName += '/1';
                    if (column.link == 'folder') {
                        actualRoute = 'file';
                    }
                }
                modal.openSelectListWindow(routeName, false, getSelectListFilter(), function (data) {

                    var id = parseInt(data.ids[0]);
                    var resource = settings.getResource(actualRoute);

                    datacontext.fetchEntitiesByKey(resource, [id], function (result) {
                        setPropertyOfSelectedItems(item, column, result.results[0]);
                    });
                });
            } else {
                toastr.warning('Can\'t open a second popup window.', 'Warning!');
            }
        };

        $scope.getPdfPreview = function (file, width, height, page) {
            return $scope.getUrl(file) + '?format=png&width=' + width + '&height=' + height + '&page=' + page + '&scale=upscalecanvas&bgcolor=white';
        };

        $scope.getImagePreview = function (file, width, height) {
            return $scope.getUrl(file) + '?width=' + width + '&height=' + height + '&scale=upscalecanvas&bgcolor=white';
        };

        $scope.getFormatedSize = function (file) {

            if (file && file.FileSize) {
                var abbr = ['bytes', 'kB', 'MB', 'GB'];

                var i = 0;
                var size = file.FileSize;
                while (size > 1024) {
                    size /= 1024;
                    ++i;
                }

                return (i == 0 ? size + ' ' : size.toFixed(2)) + abbr[i];
            }

            return file && file.FileSize;
        };


        $scope.unlink = function (item, column) {
            setPropertyOfSelectedItems(item, column, null);
        };

        $scope.onDragStart = function (item) {
            item.isSelected = true;
        };

        $scope.onDragEnd = function (item) {

        };

        function setPropertyOfSelectedItems(item, column, value) {
            _.each(_.union(item, getSelectedItems()), function (selectedItem) {
                var path = column.linkProperty.split('.');
                var currentItem = selectedItem;
                _.each(path.slice(0, -1), function (property) {
                    currentItem = currentItem[property];
                });
                currentItem[path.slice(-1)] = value;
            });
        }

        //#region Internal Methods        

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

            var id = parseInt($state.params.id);
            if (entityInfo.hierarchy && !_.isNaN(id)) {
                query = query.where(entityInfo.parentId, '==', id);
            }

            if (filters.length) {
                for (i = 0; i < filters.length; ++i) {
                    var f = filters[i];
                    query = query.where(f.property, f.condition, f.val);
                }
            }

            query = applyColumnFilters(query);
            return query;
        }

        function applyColumnFilters(query) {

            for (var i = 0; i < columns.length; ++i) {
                var column = columns[i];

                var predicate = null;

                for (var j = 0; j < column.filters.length; ++j) {
                    var filter = column.filters[j];

                    var operation = $scope.queryOperators[column.type][filter.op];

                    var currentPredicate;
                    if (column.type === 'number' || column.type === 'bool') {
                        filter.value = parseInt(filter.value);
                        currentPredicate = breeze.Predicate.create(column.property, operation.op, filter.value);
                    } else {
                        currentPredicate = breeze.Predicate.create(column.property, operation.op, '\'' + filter.value + '\'');
                    }

                    if (operation.negate) {
                        currentPredicate = breeze.Predicate.not(currentPredicate);
                    }

                    if (predicate == null) {
                        predicate = currentPredicate;
                    } else {
                        if (filter.bool == booleanAnd.value) {
                            predicate = breeze.Predicate.and([predicate, currentPredicate]);
                        } else if (filter.bool == booleanOr.value) {
                            predicate = breeze.Predicate.or([predicate, currentPredicate]);
                        }
                    }
                }

                if (predicate) {
                    query = query.where(predicate);
                }
            }

            return query;
        }

        function resetSelectedItems() {
            selectable.deselectAll();
            $scope.$emit('resetSelectedItems');
        }

        this.executeQuery = function () {
            setBusy(true);
            saveColumnsInCache();
            resetSelectedItems();
            datacontext.executeQuery(generateQuery(), entitiesLoaded, errorLoading);
        };

        var first = true;
        var columns = [];
        $scope.columns = columns;

        this.addColumn = function (column, template) {

            var index = columns.length;
            var filter = filtersFromCache.length > index && filtersFromCache[index];
            var visiblity = visibilityFromCache.length > index ? visibilityFromCache[index] : true;

            columns[column.order] = {
                name: column.name,
                link: column.link,
                linkProperty: column.linkproperty,
                order: parseInt(column.order),
                property: column.property,
                filters: filter || [],
                sort: first ? column.sort || '1' : column.sort,
                template: template,
                type: column.type,
                visible: visiblity,
                width: column.width
            };

            if (column.type == 'enumeration') {
                loadColumnFilterEnums(columns[column.order], column.enumtype);
            }

            first = false;
        };

        function loadColumnFilterEnums(column, enumType) {
            var enumQuery = datacontext.createQuery().from(enumType);
            datacontext.executeQuery(enumQuery, function (data) {
                column.enums = data.results.sort();
            });
        }

        function setBusy(value) {
            $scope.isLoading = value;
        }

        function entitiesLoaded(data) {

            $scope.count = data.inlineCount;
            $scope.items = data.results;

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

            var handle = $('table').data('resizableColumns');
            if (handle) {
                handle.destroy();
            }

            _.defer(function () {
                $('th .dropdown-menu').click(function (e) {
                    e.stopPropagation();
                });

                $('.column-selector').click(function (e) {
                    e.stopPropagation();
                });
            });
        }

        function errorLoading(error) {
            setBusy(false);
            toastr.error('Error fetching records from server.', 'Error!');
            console.error(error);
        }

        function saveColumnsInCache() {
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

        function loadSettingsFromCache() {
            var fromCache = JSON.parse(localStorage.getItem(uniqueKey));
            if (fromCache && fromCache.filters) {
                filtersFromCache = fromCache.filters;
                visibilityFromCache = fromCache.visibility;
                queryParams = fromCache.query || queryParams;
                sortedColumns = fromCache.order || sortedColumns;
            }

            $scope.queryParams = queryParams;
        }

        //#endregion

        //#region Toolbar

        var toolbar = {
            createButton: { order: 0, action: createNewItem, icon: entityInfo.multiKey ? 'fa-link' : 'fa-file-o', tooltip: entityInfo.multiKey ? 'Create new relation' : 'Create new entity', visible: false },
            copyButton: { order: 1, action: copyItems, icon: 'fa-copy', tooltip: 'Copy selected entities', visible: false },
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

        function updateButtonStates() {
            var selectedItems = getSelectedItems().length;

            toolbar.linkButton.visible = !!mode;

            if ($scope.isLoading) {
                _.each(toolbar, function (button) { button.disabled = true; });
            } else {
                toolbar.linkButton.disabled = (mode == 'one' && selectedItems != 1) || (mode == 'many' && selectedItems < 1);
                toolbar.createButton.disabled = selectedItems;
                toolbar.copyButton.disabled = !selectedItems;
                toolbar.deleteButton.disabled = !selectedItems;
                toolbar.resetButton.disabled = !selectedItems;
                toolbar.importButton.disabled = selectedItems;
                toolbar.exportButton.disabled = false;
                toolbar.refreshButton.disabled = datacontext.hasNewItems();
            }
        }

        var selectable = new Selectable($scope);

        $scope.selectItem = function (item, event, index) {
            var cursor = selectable.selectItem(item, event, index, $scope.cursor);
            if (cursor != null) {
                $scope.cursor = cursor;
            }
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

            var setCustomProperties = function (newEntity, entity) {
                if (entityInfo.to == 'file') {
                    newEntity.Name = entity.Name;
                }
            };

            var entityType = entityInfo.entityType;

            // Multikey entities should be selected from a list in a modal window and not created
            // directly
            if (entityInfo.multiKey) {
                modal.openSelectListWindow(entityInfo.toRoute || entityInfo.to, true, getSelectListFilter(), function (selection) {

                    if (selection.ids.length > 0) {

                        datacontext.fetchEntitiesByKey(entityInfo.toResource, selection.ids, function (data) {

                            var newEntities = [];
                            _.each(data.results, function (entity) {

                                var values = settings.getDefaultValuesForEntity(entityInfo.entityType);
                                values[entityInfo.fromId] = $state.params.id;
                                values[entityInfo.toProperty] = entity;

                                if (setCustomProperties) {
                                    setCustomProperties(values, entity);
                                }

                                try {
                                    newEntities.push(datacontext.createEntity(entityType, values));
                                } catch (e) {
                                    toastr.error('Item already exists.', 'Error!');
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

                var newEntity = datacontext.createEntity(entityType, values);
                $scope.items = _.union([newEntity], $scope.items);
            }
        }

        function copyItems() {
            clipboard.copy(type, getSelectedItems(), entityInfo.resource);
        }


        function importItem() {
            if (window.opener == null) {
                modal.openImportWindow(function (data) {
                    _.each(data.content.split('\n'), function (line) {
                        var cols = line.split('\t');
                        console.log(cols);
                    });
                });
            } else {
                toastr.warning('Can\'t open a second popup window.', 'Warning!');
            }
        }


        function exportItem() { }


        function deleteItems() {
            _.each(getSelectedItems(), function (item) {
                if (item.entityAspect.entityState.name !== 'Detached') {
                    item.entityAspect.setDeleted();
                    item.isSelected = false;
                }
            });
        }

        function resetItems() {
            _.each(getSelectedItems(), function (item) {
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
                filters.push({
                    property: entityInfo.fromId,
                    condition: '==',
                    val: $state.params.id
                });
                self.executeQuery();
            } else if (entityInfo.load) {
                isReady = true;
                self.executeQuery();
            }
        }

        //#endregion

        if (cacheLocally) {
            loadSettingsFromCache();
        }

        $rootScope.$watch(function () {
            updateButtonStates();
        });

        if (!filter) {
            loadGrid();
        } else {
            var query = datacontext.createQuery()
                .from(filter[0])
                .where(filter[1], '==', filter[3])
                .select(filter[2]);

            datacontext.executeQuery(query, function (data) {
                _.each(data.results, function (id) {
                    filters.push({
                        bool: booleanAnd.value,
                        property: 'Id',
                        condition: '!=',
                        val: id[filter[2]]
                    });
                });

                loadGrid();
            });
        }
    });
})();