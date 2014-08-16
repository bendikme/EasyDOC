(function () {
    'use strict';

    // Controller name is handy for logging
    var controllerId = 'tasks';

    // Define the controller on the module.
    // Inject the dependencies. 
    // Point to the controller definition function.
    angular.module('app').controller(controllerId, ['$scope', '$rootScope', '$state', 'datacontext', 'permissions', 'modal', tasks]);

    function tasks($scope, $rootScope, $state, datacontext, permissions, modal) {

        $scope.loading = true;
        $scope.items = [];

        var root;
        var selectable = null;

        var query = datacontext.createQuery().from('TaskConstraints');
        datacontext.executeQuery(query).then(function (data) {
            var array = [];
            for (var i = 0; i < data.results.length; ++i) {
                array.push({ Name: data.results[i].replace(/([A-Z])/g, " $1"), Id: data.results[i] });
            }
            $scope.constraintTypes = array;
        });

        query = datacontext.createQuery().from('TaskLinkTypes');
        datacontext.executeQuery(query).then(function (data) {
            var array = [];
            for (var i = 0; i < data.results.length; ++i) {
                array.push({ Name: data.results[i].replace(/([A-Z])/g, " $1"), Id: data.results[i] });
            }
            $scope.linkTypes = array;
        });

        $scope.$watch('ganttChartHeight', function (value) {
            gridOptions.height = value;
            setGridZoom($scope.zoom);
            ganttChart && ganttChart.invalidate();
        });


        var gridOptions = {
            width: 1000,
            height: 1000,
            subWidth: 60,
            subDivisions: 46,
            zoomLevel: 'month',
            lineHeight: 14,
            container: 'gantt-container',
            header: 'gantt-header',
            mainHeaderHeight: 32,
            subHeaderHeigh: 16,
            headerHeight: 48,
            rowHeight: 28
        }


        var ganttChart;

        function createGanttChart() {

            gridOptions.start = moment(gridOptions.projectStartDate).subtract('week', 6);
            gridOptions.end = moment(gridOptions.projectStartDate).add('week', 40);

            ganttChart = new GanttChart(gridOptions);
            ganttChart.on('dependencyCreated', function (params) {

                var link;

                if (detectCircularDependency(params.target, params.source)) {
                    toastr.error('No circular dependencies allowed.', 'Error!');
                    return;
                }

                //if (detectExistingLinkBetweenNodes(params.target, params.source)) {
                //    toastr.error('A link already exists between these tasks.', 'Error!');
                //    return;
                //}

                try {
                    link = datacontext.createEntity('TaskLink', {
                        From: params.source,
                        To: params.target,
                        Type: getLinkType(params.sourceHandle, params.targetHandle)
                    });

                    ganttChart.addDependency(link);
                    updateScope();
                } catch (e) {
                    toastr.error('This dependency link already exists.', 'Error!');
                    return;
                }
            });

            ganttChart.on('selectionChanged', function (item) {
                updateScope();
            });

            ganttChart.on('taskChanged', function () {
                updateScope();
            });

            ganttChart.on('setEmployees', function (task) {

                modal.openSelectListWindow('employee', true, '', function (selectionData) {

                    var currentEmployeeIds = _.pluck(task.Resources, 'EmployeeId');
                    var newIds = _.difference(selectionData.ids, currentEmployeeIds);
                    var deletedIds = _.difference(currentEmployeeIds, selectionData.ids);

                    _.each(deletedIds, function (idToDelete) {
                        var employee = _.findWhere(task.Resources, { EmployeeId: idToDelete });
                        employee.entityAspect.setDeleted();
                    });

                    if (newIds.length > 0) {
                        datacontext.fetchEntitiesByKey('Employees', newIds, function (serverData) {

                            _.each(serverData.results, function (employee) {
                                var entity = datacontext.createEntity('EmployeeTask', {
                                    Employee: employee,
                                    Task: task
                                });

                                task.Resources.push(entity);
                            });

                        });
                    }

                    task.gantt.item.update();
                });
            });
        }

        function detectCircularDependency(currentTask, taskToDetect) {
            return currentTask == taskToDetect || _.reduce(currentTask.Successors, function (memo, relation) {
                return memo || detectCircularDependency(relation.To, taskToDetect);
            }, false);
        }

        function detectExistingLinkBetweenNodes(targetTask, sourceTask) {
            return _.reduce(sourceTask.Successors, function (memo, relation) {
                return memo || relation.To == targetTask;
            }, false);
        }

        $scope.params = gridOptions;

        $scope.zoom = 30;
        $scope.$watch('zoom', setGridZoom);

        function setGridZoom(z) {

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

                    var zoomDate = $scope.selectedItem && $scope.selectedItem.isSelected ? $scope.selectedItem.StartDate : gridOptions.projectStartDate;

                    var c = Math.max(Math.pow(z / rule.end, 4), 0.05);

                    var before = rule.pre / c;
                    var after = rule.after / c;

                    gridOptions.zoomLevel = rule.level;
                    gridOptions.start = moment(zoomDate).subtract(rule.level, before);
                    gridOptions.end = moment(zoomDate).add(rule.level, after);
                    gridOptions.subDivisions = Math.round(before + after);
                    gridOptions.subWidth = Math.round(3000 / gridOptions.subDivisions);

                    if (ganttChart) {
                        ganttChart.drawGrid(gridOptions);
                        ganttChart.rescheduleTasks();
                    }

                    updateScope();
                }
            });
        }

        $scope.$on('selectionChanged', function (event, selection) {
            selectable = selection;
            ganttChart.invalidate();
        });

        var toolbar = {
            createButton: { order: 0, action: createNewTask, icon: 'fa-file-o', tooltip: { title: 'Create a new task', body: 'Creates a new task and makes it a child of the currently selected task, if any.' } },
            propertiesButton: { order: 1, action: taskDialog, icon: 'fa-pencil', tooltip: { title: 'Edit properties', body: 'Edit the currently selected task or dependency.' } },
            undoButton: { order: 2, action: taskDialog, icon: 'fa-undo', tooltip: { title: 'Undo', body: 'Undo the last action.' } },
            redoButton: { order: 3, action: taskDialog, icon: 'fa-repeat', tooltip: { title: 'Redo', body: 'Redo an undone action.' } },
            outdentButton: { order: 4, action: outdentTasks, icon: 'fa-outdent', tooltip: { title: 'Outdent tasks', body: 'Moves this task one level up, possibly making it a summary task.' } },
            indentButton: { order: 5, action: indentTasks, icon: 'fa-indent', tooltip: { title: 'Indent tasks', body: 'Moves this task one level down, making its predecessor a summary task' } },
            deleteButton: { order: 6, action: deleteTasks, icon: 'fa-trash-o', tooltip: { title: 'Delete selected item', body: 'Deletes the selected task or dependency.' } },
            refreshButton: { order: 7, action: refreshTasks, icon: 'fa-refresh', tooltip: { title: 'Update tasks', body: 'Updates the Gantt Chart. Normally not necessary as the chart is updated automatically.' } }
        };


        $scope.toolbar = _.toArray(toolbar);

        $scope.$on('$stateChangeStart', function (event, toState, toParams, fromState, fromParams) {
            if (datacontext.hasNewItems()) {
                toastr.warning('Save new items before leaving page', 'Warning!');
                event.preventDefault();
                $location.path($state.href(fromState, fromParams));
            }
        });

        $rootScope.$watch(function () {
            updateButtonStates();
        });


        function getLinkType(sourceHandle, targetHandle) {
            var link = sourceHandle + (targetHandle << 1);

            switch (link) {
                case 0: return 'StartToStart';
                case 1: return 'FinishToStart';
                case 2: return 'StartToFinish';
                case 3: return 'FinishToFinish';
                default: return 0;
            }
        }

        function updateScope() {
            if (!$scope.$$phase && !$scope.$root.$$phase) {
                $scope.$apply();
            }
        }

        function loadTasks() {

            query = datacontext.createQuery()
							  .from('Tasks')
							  .where('ProjectId', '==', $state.params.id)
							  .expand('Successors,Predecessors,ChildTasks,ParentTask,Resources.Employee,Project');

            datacontext.executeQuery(query, function (data) {

                if (data.results.length > 0) {

                    var rootTasks = _.filter(data.results, function (task) {
                        return task.ParentTask == null;
                    });

                    root = rootTasks.length > 0 && rootTasks[0];
                    gridOptions.projectStartDate = moment(root.Project.StartDate);

                    createGanttChart();

                    $scope.items = data.results;
                    sortGanttChart();
                    _.each($scope.items, ganttChart.addTask);
                    _.each(rootTasks, traverseDependencies);

                    ganttChart.rescheduleTasks();

                    $scope.$broadcast('loaded');
                }

                $scope.loading = false;
            }, function (error) {
                toastr.error(error.message, "Error!");
                $scope.loading = false;
            });
        }

        function traverseDependencies(item) {
            _.each(item.Successors, function (link) {
                ganttChart.addDependency(link);
            });

            _.sortBy(item.ChildTasks, function (task) {
                return task.SortOrder;
            }).forEach(traverseDependencies);
        }

        function updateButtonStates() {
            var selectedItems = selectable && selectable.getSelectedItems();
            var hasOneSelectedItem = selectable && selectedItems.length == 1;
            var hasSelectedItems = selectable && selectedItems.length >= 1;

            _.each(toolbar, function (button) { button.visible = true; });

            if ($scope.isLoading) {
                toolbar.forEach(function (button) { button.disabled = true; });
            } else {
                toolbar.createButton.disabled = selectedItems && selectedItems.length > 1;
                toolbar.propertiesButton.disabled = !hasOneSelectedItem;
                toolbar.deleteButton.disabled = !hasSelectedItems;
                toolbar.indentButton.disabled = !hasSelectedItems;
                toolbar.outdentButton.disabled = !hasSelectedItems;

                var createPermission = permissions.hasPermission('Task', 'Create');
                var modifyPermission = permissions.hasPermission('Task', 'Modify');

                toolbar.createButton.visible = createPermission;
                toolbar.propertiesButton.visible = modifyPermission;
                toolbar.indentButton.visible = modifyPermission;
                toolbar.outdentButton.visible = modifyPermission;
                toolbar.deleteButton.visible = permissions.hasPermission('Task', 'Delete');
            }
        }

        function deleteDependencies(task) {
            _.each(_.union(task.Predecessors, task.Successors), function (relation) {
                ganttChart.delete(relation);
                relation.entityAspect.setDeleted();
            });
        }

        function createNewTask() {

            var parent = selectable && selectable.getSelectedItems()[0];

            if (parent) {
                deleteDependencies(parent);
            }

            var code = getWbsCode(parent);
            var name = 'Task ' + code.replace(/0/g, '');

            var item = datacontext.createEntity('Task', {
                Name: name,
                Status: 0,
                ConstraintType: 'AsSoonAsPossible',
                ConstraintDate: moment().toDate(),
                CalendarType: 0,
                IsAutoScheduled: true,
                ParentTask: parent,
                ProjectId: $state.params.id,
                Duration: '1w',
                WbsCode: code
            });

            item.level = parent ? parent.level + 1 : 0;
            $scope.items.push(item);
            $scope.$broadcast('newItemAdded', item);

            sortGanttChart();

            ganttChart.addTask(item);
            ganttChart.rescheduleTasks();
            updateScope();
        }

        function sortGanttChart() {
            $scope.items = _.sortBy($scope.items, function (task) {
                return task.WbsCode;
            });

            _.each($scope.items, function (task, index) {
                task.level = task.WbsCode.split('.').length - 1;
                task.gantt = _.extend(task.gantt || {}, {
                    position: index
                });
            });
        }

        function refreshWbsCodes() {
            var rootTasks = _.filter($scope.items, function (task) {
                return task.ParentTask == null;
            });

            _.each(rootTasks, function (task, index) {
                generateWbsCode(task, [index + 1]);
            });

        }

        function generateWbsCode(task, code, parent) {

            task.WbsCode = _.reduce(code, function (memo, part) {
                return memo + (memo.length ? '.' : '') + padCode(part);
            }, '');

            task.ParentTask = parent;

            _.each(_.sortBy(task.ChildTasks, function (childTask) { return childTask.WbsCode; }), function (childTask, index) {
                var childCode = code.slice(0);
                childCode.push(index + 1);
                generateWbsCode(childTask, childCode, task);
            });
        }

        function padCode(code) {
            return ('0000' + code).slice(-4);
        }

        function getWbsCode(parent) {
            if (parent) {
                var prefix = parent.WbsCode;
                var suffix = padCode(parent.ChildTasks.length + 1);
                return prefix + '.' + suffix;
            } else {
                var count = _.filter($scope.items, function (task) {
                    return task.ParentTask == null;
                }).length;

                return padCode(count + 1);
            }
        }

        $scope.selectedItemCopy = {};
        var taskProperties = ['IsAutoScheduled', 'IsInactive', 'IsMilestone', 'ConstraintDate', 'ConstraintType', 'Duration', 'EndDate', 'Name', 'PercentComplete', 'Priority', 'StartDate'];
        var linkProperties = ['Lag', 'Type', 'From', 'To'];

        function taskDialog() {

            var items = selectable && selectable.getSelectedItems();
            var item = items && items.length == 1 && items[0];

            if (item) {
                copyProperties(item, $scope.selectedItemCopy, taskProperties);
                $scope.selectedItemCopy.summaryTask = item.ChildTasks.length > 0;
                $('#task-dialog').modal('show');
            } else {
                copyProperties(item, $scope.selectedItemCopy, linkProperties);
                $('#link-dialog').modal('show');
            }
        }

        $scope.rescheduleTasks = function (update) {

            var items = selectable && selectable.getSelectedItems();
            var item = items && items.length == 1 && items[0];

            if (update) {
                if (item) {
                    copyProperties($scope.selectedItemCopy, item, taskProperties);
                } else {

                    if ($scope.selectedItemCopy.Type != item.Type) {

                        ganttChart.delete(item);

                        try {
                            var link = datacontext.createEntity('TaskLink', $scope.selectedItemCopy);
                            ganttChart.addDependency(link);
                            updateScope();
                        } catch (e) {
                            toastr.error('This dependency link already exists.', 'Error!');
                            return;
                        }
                    } else {
                        copyProperties($scope.selectedItemCopy, item, linkProperties);
                    }
                }

                ganttChart.rescheduleTasks();
                updateScope();
            }

            $('#task-dialog').modal('hide');
            $('#link-dialog').modal('hide');
        };

        function copyProperties(source, target, properties) {
            _.each(properties, function (property) {
                target[property] = source[property];
            });
        }

        function deleteTasks() {

            var items = selectable && selectable.getSelectedItems();
            var affectedParents = [];

            _.each(items, function (item) {
                ganttChart.delete(item);
                affectedParents.push(item.ParentTask);
                item.entityAspect.setDeleted();
            });

            ganttChart.rescheduleTasks();

            _.each(affectedParents, function (parent) {
                if (parent != null) {
                    parent.gantt.item.update();
                }
            });

            updateScope();
        }


        function indentTasks() {

            var items = selectable && selectable.getSelectedItems();
            var item = items && items.length == 1 && items[0];

            if (item) {
                var code = item.WbsCode.split('.');

                var lastPart = parseInt(code[code.length - 1]);

                if (lastPart > 1) {

                    var index = _.indexOf($scope.items, item);
                    var previousTask = $scope.items[index - 1];

                    if (previousTask.level == item.level) {

                        deleteDependencies(previousTask);
                        item.ParentTask = previousTask;
                        ganttChart.redrawTask(previousTask);

                    } else if (previousTask.level > item.level) {

                        var part = _.first($scope.items, index);
                        previousTask = _.last(_.filter(part, function (task) { return task.level == item.level + 1; }));

                        item.ParentTask = previousTask.ParentTask;
                        item.WbsCode = 'x';
                    }

                    refreshWbsCodes();
                    sortGanttChart();
                    ganttChart.rescheduleTasks();
                    updateScope();
                }
            }
        }


        function outdentTasks() {

            var items = selectable && selectable.getSelectedItems();
            var item = items && items.length == 1 && items[0];

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
                    deleteDependencies(item);
                    ganttChart.redrawTask(item);
                }

                if (previousParent.ChildTasks.length == 0) {
                    ganttChart.redrawTask(previousParent);
                }

                refreshWbsCodes();
                sortGanttChart();
                ganttChart.rescheduleTasks();
                updateScope();
            }
        }

        function refreshTasks() {
            ganttChart.rescheduleTasks();
            updateScope();
        }

        loadTasks();
    }

})();