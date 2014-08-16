/**
 * Use of Class.js
 */

var AbstractGridController = BaseController.extend({
    _columns: null,
    _configuration: null,
    _editMode: false,
    _isReady: false,
    _notifications: null,
    _parentId: null,
    _parentController: null,
    _permissions: null,
    _sortedColumns: [],

    _listeners: {},

    _columnSelection: null,
    selection: null,
    filters: new FilterCollection({
        property: 'Id',
        type: 'number'
    }),

    /**
     * Initialize Abstract Grid Controller
     * @param $scope, current controller scope
     */
    init: function ($rootScope, $scope, Notifications, permissions) {

        this._columns = null;
        this._configuration = null;
        this._editMode = false;
        this._isReady = false;
        this._parentId = null;
        this._sortedColumns = [];

        this.filters = new FilterCollection({
            property: 'Id',
            type: 'number'
        });

        this._notifications = Notifications;
        this._permissions = permissions;
        this._columnSelection = new Selectable(this, '_columns');
        this.selection = new Selectable($scope, 'items');

        this._super($scope);

        permissions.whenReady(this._load.bind(this));
    },

    /**
     *@Override
     */
    defineListeners: function () {
        this._listeners = {
            'ui.navigation.events.UP': this.selectPreviousItem.bind(this),
            'ui.navigation.events.DOWN': this.selectNextItem.bind(this),
            'ui.navigation.events.LEFT': this.selectPreviousColumn.bind(this),
            'ui.navigation.events.RIGHT': this.selectNextColumn.bind(this),
            'ui.navigation.events.SPACE': this._startEditCurrentCell.bind(this)
        };

        this._super();
    },

    /**
     *@Override
     */
    defineScope: function () {

        this._super();

        this.$scope.cursor = 0;

        this.$scope.items = [];
        this.$scope.columns = this._columns;
        this.$scope.count = 0;

        this.$scope.showToolbar = !this._configuration.hideToolbar;
        this.$scope.showStatusbar = !this._configuration.hideStatusbar;
        this.$scope.rowClass = this._configuration.rowClass;

        this.$scope.isLoading = true;
        this.$scope.isEditable = false;

        this.$scope.isEditing = this._isEditingCell.bind(this);
        this.$scope.startEditing = this._startEditingCell.bind(this);
        this.$scope.stopEditing = this._stopEditingCell.bind(this);
        this.$scope.saveEditedItem = this._saveEditedItem.bind(this);

        this.$scope.link = this._link.bind(this);
        this.$scope.unlink = this._unlink.bind(this);

        this.$scope.selectedColumn = null;
        this.$scope.booleanOperators = [FilterCollection.booleanAnd, FilterCollection.booleanOr];

        this.$scope.resetColumns = this._resetColumns.bind(this);
        this.$scope.showFilterDialog = this._showFilterDialog.bind(this);
        this.$scope.toggleColumn = this._toggleColumn.bind(this);
        this.$scope.orderByProperty = this._orderByProperty.bind(this);
        this.$scope.requeryWithFilters = this._refresh.bind(this);

        this.$scope.selectItem = this._selectItem.bind(this);
        this.$scope.selectAll = this._selectAll.bind(this);

        // Temporary value when editing cell
        this.$scope.temp = null;
    },

    _load: function () {

        _.each(this._columns, function (column) {
            if (column.type == 'enumeration') {
                column.enums = this._permissions.lookup[column.enumtype];
                column.isSelected = false;
            }
        }, this);

        this._columns[0].isSelected = true;
    },

    addItem: function (item) {
        this._extendItemWithTemplate(item);
        this.$scope.items.unshift(item);
    },

    selectPreviousItem: function (name, event) {
        if (!this._editMode && this.$scope.cursor > 0) {
            this._updateScope(function () {
                this._selectItem(this.$scope.items[--this.$scope.cursor], null, event);
            }.bind(this));
            event.preventDefault();
        }
    },

    selectNextItem: function (name, event) {
        if (!this._editMode && this.$scope.cursor < this.$scope.items.length - 1) {
            this._updateScope(function () {
                this._selectItem(this.$scope.items[++this.$scope.cursor], null, event);
            }.bind(this));
            event.preventDefault();
        }
    },

    selectPreviousColumn: function (name, event) {
        if (!this._editMode) {
            this._updateScope(function () {
                this._columnSelection.selectPrevious();
            }.bind(this));
            event.preventDefault();
        }
    },

    selectNextColumn: function (name, event) {
        if (!this._editMode) {
            this._updateScope(function () {
                this._columnSelection.selectNext();
            }.bind(this));
            event.preventDefault();
        }
    },

    _startEditCurrentCell: function (name, event) {

        var items = this.selection.getSelectedItems();
        var columns = this._columnSelection.getSelectedItems();

        if (items.length == 1 && columns.length == 1) {

            var item = items[0];
            var column = columns[0];

            if (!this._isEditingCell(item, column)) {
                this._updateScope(function () {
                    if (item._columns[column.name].permission && !column.readonly && !column.link) {
                        this._startEditingCell(items[0], columns[0], event);
                    }
                }.bind(this));
            }
        }
    },

    _showFilterDialog: function (column) {
        this.$scope.selectedColumn = column;
        $('#filter-dialog').modal('show');
    },

    _selectItem: function (item, column, event) {
        this.$scope.cursor = this.selection.selectItem(item, event, this.$scope.cursor);

        if (column) {
            this._columnSelection.deselectAll();
            column.isSelected = true;
        }
    },

    _selectAll: function (event) {
        if (event.target.checked) {
            this.selection.selectAll();
        } else {
            this.selection.deselectAll();
        }
    },

    _clearSelection: function () {
        this.selection.deselectAll();
    },

    _link: function (item, column, event) {
        event.preventDefault();
        if (this._parentController && this._parentController.openModalLinkWindow) {
            this._parentController.openModalLinkWindow(item, column);
        }
    },

    _unlink: function (item, column) {
        this._setPropertyOfSelectedItems(item, column, null);
    },

    _setPropertyOfSelectedItems: function (item, column, value) {

        _.each(_.union(item, this.selection.getSelectedItems()), (function (selectedItem) {

            var path = column.linkProperty.split('.');
            var currentItem = selectedItem;

            path.slice(0, -1).forEach(function (property) {
                currentItem = currentItem[property];
            });

            currentItem[path.slice(-1)] = value;
        }));
    },

    _toggleColumn: function (column) {
        column.visible = !column.visible;
    },

    _isEditingCell: function (item, column) {
        return item.editMode && column.editMode;
    },

    _saveEditedItem: function (item, column, event) {
        this._stopEditingCell(item, column);
        item[column.property] = this.$scope.temp.value;
        event && event.preventDefault();
    },

    _startEditingCell: function (item, column, event) {

        this._editMode = true;

        this.$scope.temp = {
            value: item[column.property]
        };

        item.editMode = true;
        column.editMode = true;

        if (column.type == 'enumeration') {
            var options = this._permissions.lookup[column.enumtype];
            var id = 0;
            this.$scope.options = _.map(options, function (option) {
                return {
                    Id: id++,
                    Name: option
                };
            });
        }

        event && event.preventDefault();
    },

    _stopEditingCell: function (item, column) {
        this._editMode = false;
        item.editMode = false;
        column.editMode = false;
    },

    _refresh: function () {

    },

    _toggleSortDirection: function (column) {
        column.direction *= -1;
    },

    _orderByProperty: function (property, event) {

        var columnAlreadySorted = this._sortedColumns.some(function (item) {
            return item.property === property;
        });

        var hasChangedSorting = false;

        if (event && event.ctrlKey && this._sortedColumns.length) {
            var lastSorted = this._sortedColumns[this._sortedColumns.length - 1];

            if (lastSorted.property === property) {
                this._toggleSortDirection(lastSorted);
                if (lastSorted.direction == 0) {
                    this._sortedColumns.pop();
                }
                hasChangedSorting = true;
            } else if (!columnAlreadySorted) {
                this._sortedColumns.push({ property: property, direction: 1 });
                hasChangedSorting = true;
            }
        } else {
            if (this._sortedColumns.length == 1 && columnAlreadySorted) {
                this._toggleSortDirection(this._sortedColumns[0]);
                if (this._sortedColumns[0].direction == 0) {
                    this._sortedColumns.pop();
                }
            } else {
                this._sortedColumns = new Array({ property: property, direction: 1 });
            }
            hasChangedSorting = true;
        }

        for (var j in this._columns) {

            var unsorted = true;

            for (var i in this._sortedColumns) {
                if (this._columns[j].property == this._sortedColumns[i].property) {
                    this._columns[j].sort = this._sortedColumns[i].direction;
                    unsorted = false;
                    break;
                }
            }

            if (unsorted) {
                this._columns[j].sort = 0;
            }
        }

        return hasChangedSorting;
    },

    _resetColumns: function () {
        _.each(this._columns, function (col) {
            col.visible = true;
        });

        this._columns.sort(function(a, b) {
            return a.order > b.order;
        });
    },

    _prepareColumns: function () {

        // Add additional fields to the columns
        _.each(this._columns, function (column, index) {
            column.order = column.order || index;
            column.visible = column.visible || true;
            column.property = column.property || column.name;
            column.filterCollection = column.filterCollection || new FilterCollection(column);
        });
    },

    _getModelInfo: function (item, column) {

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
    },

    _getPermission: function (info, route, item, column) {
        return (column.basePermission || info) ? this._permissions.hasPermission(column.basePermission ? this._getEntityType(item) : route, 'Modify', column.basePermission ? item.CreatorId : info.model.CreatorId) : false;
    },

    _getTemplate: function (info, route, item, column) {

        var value = column.formatter ? '{{' + column.formatter + '(item.' + column.property + ', item)}}' : '{{item.' + column.property + '}}';

        if (column.url) {
            column.route = route;
            return '<span class="icn-indent ' + route + '"><a href="/' + route + '/{{item.' + column.url + '}}" title="' + value + '">' + value + '</a></span>';
        }

        return '<span title="{{item.' + column.property + '}}">' + value + '</span>';
    },

    _getEntityType: function (model) {
        return model.entityAspect._entityKey.entityType.shortName.toLowerCase();
    },

    _extendItemWithTemplate: function (item) {

        var templateConfig = {};
        _.each(this._columns, (function (col) {

            var info = this._getModelInfo(item, col);
            var route = info && this._getEntityType(info.model);

            templateConfig[col.name] = {
                title: info ? info.value : '',
                template: this._getTemplate(info, route, item, col),
                permission: this._getPermission(info, route, item, col)
            };
        }), this);

        item._columns = templateConfig;
        return item;
    },

    setBusy: function (value) {
        this.$scope.isLoading = value;
    }

});

AbstractGridController.$inject = ['$rootScope', '$scope', 'Notifications', 'permissions'];