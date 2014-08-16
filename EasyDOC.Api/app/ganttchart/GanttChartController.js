
(function () {

    var GanttChartController = BaseController.extend({
        _commands: null,
        _datacontext: null,
        _entityInfo: null,
        _modal: null,
        _notifications: null,
        _permissions: null,
        _settings: null,
        _selectable: null,

        _ganttChart: null,
        _ganttChartOptions: {
            width: 1000,
            height: 1000,
            subWidth: 60,
            subDivisions: 46,
            zoomLevel: 'month',
            lineHeight: 14,
            container: 'gantt-container',
            header: 'gantt-header',
            mainHeaderHeight: 24,
            subHeaderHeigh: 15,
            headerHeight: 39,
            rowHeight: 38
        },

        _toolbar: null,
        _taskProperties: ['IsAutoScheduled', 'IsInactive', 'IsMilestone', 'ConstraintDate', 'ConstraintType', 'Duration', 'EndDate', 'Name', 'PercentComplete', 'Priority', 'StartDate'],
        _linkProperties: ['Lag', 'Type', 'From', 'To'],

        $rootScope: null,

        init: function ($rootScope, $scope, $state, $location, notifications, datacontext, modal, permissions, settings, commands) {

            this._commands = commands;
            this._datacontext = datacontext;
            this._modal = modal;
            this._notifications = notifications;
            this._permissions = permissions;
            this._settings = settings;
            this._entityInfo = _.findWhere(settings.queryParams, { route: 'tasks' });

            this.$location = $location;
            this.$rootScope = $rootScope;
            this.$state = $state;

            this._toolbar = {
                toggleGridButton: { order: -1, action: this._toggleGrid.bind(this), icon: 'fa-list', tooltip: { title: 'Tabell' } },
                createButton: { order: 0, action: this._createNewTask.bind(this), icon: 'fa-file-o', tooltip: { title: 'Ny oppgave' } },
                propertiesButton: { order: 1, action: this._taskDialog.bind(this), icon: 'fa-pencil', tooltip: { title: 'Rediger' } },
                //undoButton: { order: 2, action: this._taskDialog.bind(this), icon: 'fa-undo', tooltip: { title: 'Angre' } },
                //redoButton: { order: 3, action: this._taskDialog.bind(this), icon: 'fa-repeat', tooltip: { title: 'Gjør om' } },
                outdentButton: { order: 4, action: this._outdentTasks.bind(this), icon: 'fa-outdent', tooltip: { title: 'Nivå ned' } },
                indentButton: { order: 5, action: this._indentTasks.bind(this), icon: 'fa-indent', tooltip: { title: 'Nivå opp' } },
                deleteButton: { order: 6, action: this._deleteTasks.bind(this), icon: 'fa-trash-o', tooltip: { title: 'Slett' } },
                refreshButton: { order: 7, action: this.refreshTasks.bind(this), icon: 'fa-refresh', tooltip: { title: 'Oppdater' } },
                saveButton: { order: 8, action: this._saveChanges.bind(this), icon: 'fa-save', tooltip: { title: 'Lagre' } },
                cancelButton: { order: 9, action: this._cancelChanges.bind(this), icon: 'fa-rotate-left', tooltip: { title: 'Avbryt' }, visible: false }
            };

            this._super($scope, $location, $state, datacontext);

            var that = this;

            $scope.$on('gridReady', function (event, gridController) {

                var config = settings.detailViewSettings.tasks;
                var columns = config.columns;
                var parentId = $state.params.id;

                that._gridController = gridController;
                that._gridController.filters.add('ProjectId', FilterCollection.booleanAnd.value, 'Equals', parentId);
                that._gridController._setConfiguration([parentId, config, columns, that._entityInfo, that, true]);
                that._notifications.notify('performPostInit');

                that._selectable = that._gridController.selection;

                that._updateButtonStates();
            });

            this._permissions.whenReady(this.initGanttChart.bind(this));
            this._loadTasks();
        },

        defineScope: function () {
            this._super();
            this.$scope.showGrid = true;
            this.$scope.loading = true;
            this.$scope.items = [];
            this.$scope.selectedItemCopy = {};
            this.$scope.params = this._ganttChartOptions;
            this.$scope.toolbar = _.toArray(this._toolbar);
            this.$scope.zoom = 30;

            this.$scope.rescheduleTasks = this._rescheduleTasks.bind(this);
            this.$scope.getAsync = this._getAsync.bind(this);
            this.$scope.addResource = this._addResource.bind(this);
            this.$scope.addCheckPoint = this._addCheckPoint.bind(this);
        },

        defineListeners: function () {
            this._super();
            this.$scope.$watch('zoom', this._setGridZoom.bind(this));
            this._notifications.addEventListener('ganttChartHeightChanged', this._onGanttChartHeightChanged.bind(this));
        },

        _getAsync: function (from, search, unique) {

            if (_.isString(search) && search.length > 0) {

                var query = this._datacontext.createQuery()
                    .from(from)
                    .where('Name', 'Contains', search)
                    .orderBy('Name')
                    .take(10);

                var that = this;

                return this._datacontext.executeQuery(query, true)
                    .then(function (data) {
                        var selectedEmployees = unique ? _.pluck(that.$scope.selectedItem.Resources, 'Employee') : [];
                        return _.difference(data.results, selectedEmployees);
                    });
            }

            return [];
        },


        _getSelectedItem: function () {
            var items = this._selectable && this._selectable.getSelectedItems();
            return items && items.length == 1 && items[0];
        },

        _addCheckPoint: function (name, resource) {

            var item = this._getSelectedItem();

            if (item) {

                this._datacontext.createEntity('TaskCheckPoint', {
                    Task: item,
                    Name: name,
                    Completed: false,
                    Employee: resource
                });

                this.$scope.resource = null;
            }
        },


        _addResource: function (resource) {

            var item = this._getSelectedItem();

            if (item) {

                var entities = this._datacontext.getChanges('EmployeeTask', breeze.EntityState.Deleted);
                var existingItem = _.findWhere(entities, { TaskId: item.Id, EmployeeId: resource.Id });

                if (existingItem) {
                    /* BUG: This does not work if the existingItem already was attached when loading the model
				     In order to fix this, we need to save the state of the entity prior to deleting it and
                     restore its state upon reattaching it. */
                    this._datacontext.detachEntity(existingItem);
                    this._datacontext.addEntity(existingItem);
                } else {
                    this._datacontext.createEntity('EmployeeTask', {
                        Task: item,
                        Employee: resource
                    });
                }

                this.$scope.resource = null;
            }
        },


        _onSelectionChanged: function (event, selection) {
            console.log(event, selection);
            this._ganttChart.invalidate();
        },

        initGanttChart: function () {

            this.$scope.constraintTypes = _.map(this._permissions.lookup.TaskConstraints, function (type) {
                return { Name: type.replace(/([A-Z])/g, " $1"), Id: type };
            });

            this.$scope.linkTypes = _.map(this._permissions.lookup.TaskLinkTypes, function (type) {
                return { Name: type.replace(/([A-Z])/g, " $1"), Id: type };
            });
        },

        _onGanttChartHeightChanged: function (event, value) {
            this._ganttChartOptions.height = value;
            console.log(value);
            this._setGridZoom(this.$scope.zoom);
            if (value > 0) {
                this._ganttChart && this._ganttChart.invalidate();
            }
        },


        _createGanttChart: function () {

            this._ganttChartOptions.start = moment(this._ganttChartOptions.projectStartDate).subtract('week', 6);
            this._ganttChartOptions.end = moment(this._ganttChartOptions.projectStartDate).add('week', 40);

            var that = this;

            this._ganttChart = new GanttChart(this._ganttChartOptions);
            this._ganttChart.on('dependencyCreated', function (params) {

                var link;

                if (that._detectCircularDependency(params.target, params.source)) {
                    toastr.error('No circular dependencies allowed.', 'Error!');
                    return;
                }

                //if (that._detectExistingLinkBetweenNodes(params.target, params.source)) {
                //    toastr.error('A link already exists between these tasks.', 'Error!');
                //    return;
                //}

                try {
                    link = that._datacontext.createEntity('TaskLink', {
                        From: params.source,
                        To: params.target,
                        Type: that._getLinkType(params.sourceHandle, params.targetHandle)
                    });

                    that._ganttChart.addDependency(link);
                    that._updateScope();
                } catch (e) {
                    toastr.error('This dependency link already exists.', 'Error!');
                    return;
                }
            });

            this._ganttChart.on('selectionChanged', function (event, selection) {
                console.log(event, selection);
                that._updateScope();
            });

            this._ganttChart.on('taskChanged', function () {
                that._updateScope();
            });

        },

        _detectCircularDependency: function (currentTask, taskToDetect) {
            return currentTask == taskToDetect || _.reduce(currentTask.Successors, function (memo, relation) {
                return memo || this._detectCircularDependency(relation.To, taskToDetect);
            }, false, this);
        },

        _detectExistingLinkBetweenNodes: function (targetTask, sourceTask) {
            return _.reduce(sourceTask.Successors, function (memo, relation) {
                return memo || relation.To == targetTask;
            }, false);
        },

        _toggleGrid: function () {
            this.$scope.showGrid = !this.$scope.showGrid;
        },

        _setGridZoom: function (z) {

            var zoomRules = [
                {
                    start: 0,
                    end: 33,
                    pre: 3,
                    after: 10,
                    level: 'month'
                },
                {
                    start: 33,
                    end: 80,
                    pre: 1.2,
                    after: 3,
                    level: 'week'
                },
                {
                    start: 80,
                    end: 160,
                    pre: 1.5,
                    after: 3.6,
                    level: 'day'
                }
            ];

            _.each(zoomRules, function (rule) {
                if (z >= rule.start && z < rule.end) {

                    var zoomDate = this.$scope.selectedItem && this.$scope.selectedItem.isSelected ? this.$scope.selectedItem.StartDate : this._ganttChartOptions.projectStartDate;

                    var c = Math.max(Math.pow(z / rule.end, 4), 0.05);

                    var before = rule.pre / c;
                    var after = rule.after / c;

                    this._ganttChartOptions.zoomLevel = rule.level;
                    this._ganttChartOptions.start = moment(zoomDate).subtract(rule.level, before);
                    this._ganttChartOptions.end = moment(zoomDate).add(rule.level, after);
                    this._ganttChartOptions.subDivisions = Math.round(before + after);
                    this._ganttChartOptions.subWidth = Math.round(3000 / this._ganttChartOptions.subDivisions);

                    if (this._ganttChart) {
                        this._ganttChart.drawGrid(this._ganttChartOptions);
                        this._ganttChart.rescheduleTasks();
                    }
                }
            }, this);
        },


        _getLinkType: function (sourceHandle, targetHandle) {
            var link = sourceHandle + (targetHandle << 1);

            switch (link) {
                case 0:
                    return 'StartToStart';
                case 1:
                    return 'FinishToStart';
                case 2:
                    return 'StartToFinish';
                case 3:
                    return 'FinishToFinish';
                default:
                    return 0;
            }
        },

        _saveChanges: function () {
            this._datacontext.saveChanges();
        },

        _cancelChanges: function () {
            this._datacontext.cancelChanges();
        },

        _loadTasks: function () {

            var that = this;
            var root;

            var query = this._datacontext.createQuery()
                .from('Tasks')
                .where('ProjectId', '==', this.$state.params.id)
                .expand('CheckPoints,CheckPoints.Employee,Successors,Predecessors,ChildTasks,ParentTask,Resources.Employee,Project');

            this._datacontext.executeQuery(query)
                .then(function (data) {
                    var rootTasks = _.filter(data.results, function (task) {
                        return task.ParentTask == null;
                    });

                    root = rootTasks.length > 0 && rootTasks[0];
                    that._ganttChartOptions.projectStartDate = moment(root ? root.Project.StartDate : new Date());

                    that._createGanttChart();

                    that.$scope.items = data.results;
                    that._sortGanttChart();
                    _.each(that.$scope.items, that._ganttChart.addTask);
                    _.each(rootTasks, that._traverseDependencies, that);

                    that._ganttChart.rescheduleTasks();
                    that._notifications.notify('itemsLoaded', that.$scope.items);

                    that.$rootScope.$watch(that._updateButtonStates.bind(that));
                }).catch(function (error) {
                    toastr.error(error.message, "Error!");
                }).finally(function () {
                    that.$scope.loading = false;
                });
        },

        _traverseDependencies: function (item) {
            _.each(item.Successors, function (link) {
                this._ganttChart.addDependency(link);
            }, this);

            _.each(item.ChildTasks, function (task) {
                this._traverseDependencies(task);
            }, this);
        },

        _updateButtonStates: function () {

            var selectedItems = this._selectable && this._selectable.getSelectedItems();
            var hasOneSelectedItem = this._selectable && selectedItems.length == 1;
            var hasSelectedItems = this._selectable && selectedItems.length >= 1;

            _.each(this._toolbar, function (button) { button.visible = true; });

            this._toolbar.createButton.disabled = selectedItems && selectedItems.length > 1;
            this._toolbar.propertiesButton.disabled = !hasOneSelectedItem;
            this._toolbar.deleteButton.disabled = !hasSelectedItems;
            this._toolbar.indentButton.disabled = !hasSelectedItems;
            this._toolbar.outdentButton.disabled = !hasSelectedItems;

            var createPermission = this._permissions.hasPermission('Task', 'Create');
            var modifyPermission = this._permissions.hasPermission('Task', 'Modify');

            this._toolbar.createButton.visible = createPermission;
            this._toolbar.propertiesButton.visible = modifyPermission;
            this._toolbar.indentButton.visible = modifyPermission;
            this._toolbar.outdentButton.visible = modifyPermission;
            this._toolbar.deleteButton.visible = this._permissions.hasPermission('Task', 'Delete');

            this._toolbar.saveButton.visible = this._toolbar.cancelButton.visible = this._datacontext.hasChanges();
        },

        _deleteDependencies: function (task) {
            _.each(_.union(task.Predecessors, task.Successors), function (relation) {
                this._ganttChart.delete(relation);
                relation.entityAspect.setDeleted();
            }, this);
        },

        _createNewTask: function () {

            var parent = this._selectable && this._selectable.getSelectedItems()[0];

            if (parent) {
                this._deleteDependencies(parent);
            }

            var code = this._getWbsCode(parent);
            var name = 'Oppgave ' + code.replace(/0/g, '');

            var item = this._datacontext.createEntity('Task', {
                Name: name,
                Status: 0,
                ConstraintType: 'AsSoonAsPossible',
                ConstraintDate: moment().toDate(),
                CalendarType: 0,
                IsAutoScheduled: true,
                ParentTask: parent,
                ProjectId: this.$state.params.id,
                Duration: '1w',
                WbsCode: code
            });

            item.level = parent ? parent.level + 1 : 0;
            this.$scope.items.push(item);
            this._notifications.notify('newItemAdded', item);

            this._sortGanttChart();

            this._ganttChart.addTask(item);
            this._ganttChart.rescheduleTasks();
            this._updateScope();
        },

        _sortGanttChart: function () {
            this.$scope.items = _.sortBy(this.$scope.items, function (task) {
                return task.WbsCode;
            });

            _.each(this.$scope.items, function (task, index) {
                task.level = task.WbsCode.split('.').length - 1;
                task.gantt = _.extend(task.gantt || {}, {
                    position: index
                });
            });
        },

        _refreshWbsCodes: function () {

            var rootTasks = _.filter(this.$scope.items, function (task) {
                return task.ParentTask == null;
            });

            _.each(rootTasks, function (task, index) {
                this._generateWbsCode(task, [index + 1]);
            }, this);

        },

        _generateWbsCode: function (task, code, parent) {

            task.WbsCode = _.reduce(code, function (memo, part) {
                return memo + (memo.length ? '.' : '') + this._padCode(part);
            }, '', this);

            task.ParentTask = parent;

            _.each(_.sortBy(task.ChildTasks, function (childTask) { return childTask.WbsCode; }), function (childTask, index) {
                var childCode = code.slice(0);
                childCode.push(index + 1);
                this._generateWbsCode(childTask, childCode, task);
            }, this);
        },

        _padCode: function (code) {
            return ('0000' + code).slice(-4);
        },

        _getWbsCode: function (parent) {
            if (parent) {
                var prefix = parent.WbsCode;
                var suffix = this._padCode(parent.ChildTasks.length + 1);
                return prefix + '.' + suffix;
            } else {
                var count = _.filter(this.$scope.items, function (task) {
                    return task.ParentTask == null;
                }).length;

                return this._padCode(count + 1);
            }
        },

        _taskDialog: function () {

            var item = this._getSelectedItem();
            this.$scope.selectedItem = item;

            if (item) {
                this._copyProperties(item, this.$scope.selectedItemCopy, this._taskProperties);
                this.$scope.selectedItemCopy.summaryTask = item.ChildTasks.length > 0;
                $('#task-dialog').modal('show');
            } else {
                this._copyProperties(item, this.$scope.selectedItemCopy, this._linkProperties);
                $('#link-dialog').modal('show');
            }
        },

        _rescheduleTasks: function (avoidUpdate) {

            var item = this._getSelectedItem();

            if (!avoidUpdate) {
                if (item) {
                    this._copyProperties(this.$scope.selectedItemCopy, item, this._taskProperties);
                } else {

                    if (this.$scope.selectedItemCopy.Type != item.Type) {

                        this._ganttChart.delete(item);

                        try {
                            var link = this._datacontext.createEntity('TaskLink', $scope.selectedItemCopy);
                            this._ganttChart.addDependency(link);
                            this._updateScope();
                        } catch (e) {
                            toastr.error('This dependency link already exists.', 'Error!');
                            return;
                        }
                    } else {
                        this._copyProperties(this.$scope.selectedItemCopy, item, this._linkProperties);
                    }
                }

                this._ganttChart.rescheduleTasks();
                this._updateScope();
            }

            $('#task-dialog').modal('hide');
            $('#link-dialog').modal('hide');
        },

        _copyProperties: function (source, target, properties) {
            _.each(properties, function (property) {
                target[property] = source[property];
            });
        },

        _deleteTasks: function () {

            var items = this._selectable && this._selectable.getSelectedItems();
            var affectedParents = [];

            _.each(items, function (item) {
                this._ganttChart.delete(item);
                affectedParents.push(item.ParentTask);
                item.entityAspect.setDeleted();
            }, this);

            this._ganttChart.rescheduleTasks();

            _.each(affectedParents, function (parent) {
                if (parent != null) {
                    parent.gantt.item.update();
                }
            });

            this._updateScope();
        },


        _indentTasks: function () {

            var item = this._getSelectedItem();

            if (item) {
                var code = item.WbsCode.split('.');

                var lastPart = parseInt(code[code.length - 1]);

                if (lastPart > 1) {

                    var index = _.indexOf(this.$scope.items, item);
                    var previousTask = this.$scope.items[index - 1];

                    if (previousTask.level == item.level) {

                        this._deleteDependencies(previousTask);
                        item.ParentTask = previousTask;
                        this._ganttChart.redrawTask(previousTask);

                    } else if (previousTask.level > item.level) {

                        var part = _.first(this.$scope.items, index);
                        previousTask = _.last(_.filter(part, function (task) { return task.level == item.level + 1; }));

                        item.ParentTask = previousTask.ParentTask;
                        item.WbsCode = 'x';
                    }

                    this._refreshWbsCodes();
                    this._sortGanttChart();
                    this._ganttChart.rescheduleTasks();
                    this._updateScope();
                }
            }
        },

        _outdentTasks: function () {

            var item = this._getSelectedItem();

            if (item && item.level > 0) {

                var previousParent = item.ParentTask;
                var children = _.sortBy(previousParent.ChildTasks, function (task) {
                    return task.WbsCode;
                });

                var startIndex = _.indexOf(children, item);
                _.each(children, function (task, index) {
                    if (index > startIndex) {
                        task.ParentTask = item;
                    }
                });

                item.ParentTask = previousParent.ParentTask;

                if (item.ChildTasks.length > 0) {
                    this._deleteDependencies(item);
                    this._ganttChart.redrawTask(item);
                }

                if (previousParent.ChildTasks.length == 0) {
                    this._ganttChart.redrawTask(previousParent);
                }

                this._refreshWbsCodes();
                this._sortGanttChart();
                this._ganttChart.rescheduleTasks();
                this._updateScope();
            }
        },

        refreshTasks: function () {
            this._ganttChart.rescheduleTasks();
            this._updateScope();
        }
    });

    angular.module('app')
        .controller('GanttChartController',
            ['$rootScope', '$scope', '$state', '$location', 'Notifications', 'datacontext', 'modal', 'permissions', 'settings', 'commands', GanttChartController]);
})();