(function () {
    'use strict';

    // Controller name is handy for logging
    var controllerId = 'tasks';

    // Define the controller on the module.
    // Inject the dependencies. 
    // Point to the controller definition function.
    angular.module('app').controller(controllerId, ['$scope', '$rootScope', '$state', 'datacontext', 'permissions', 'settings', tasks]);

    function tasks($scope, $rootScope, $state, datacontext, permissions, settings) {

        var toolbar = {
            createButton: { order: 0, action: createNewTask, icon: 'fa-file-o', tooltip: 'Create a new task' },
            copyButton: { order: 1, action: copyTasks, icon: 'fa-copy', tooltip: 'Copy selected tasks' },
            pasteButton: { order: 2, action: pasteTasks, icon: 'fa-paste', tooltip: 'Paste from clipboard' },
            deleteButton: { order: 3, action: deleteTasks, icon: 'fa-trash-o', tooltip: 'Delete selected tasks' },
            resetButton: { order: 4, action: resetTasks, icon: 'fa-retweet', tooltip: 'Reset selected tasks' },
            refreshButton: { order: 5, action: refreshTasks, icon: 'fa-refresh', tooltip: 'Refresh tasks' }
        };


        $scope.$on('$stateChangeStart', function (event, toState, toParams, fromState, fromParams) {
            if (datacontext.hasNewItems()) {
                toastr.warning('Save new items before leaving page', 'Warning!');
                event.preventDefault();
                $location.path($state.href(fromState, fromParams));
            }
        });


        var items = {
            data: [],
            links: []
        };

        var getGanttType = function (type) {
            switch (type) {
                case "Project":
                    return gantt.config.types.project;

                case "Milestone":
                    return gantt.config.types.milestone;

                default:
                    return gantt.config.types.task;
            }
        };



        gantt.config.details_on_create = true;

        gantt.templates.rightside_text = function (start, end, task) {
            if (task.type == gantt.config.types.milestone) {
                return task.text;
            }
            return "";
        };

        gantt.templates.leftside_text = function (start, end, task) {
            if (task.type == gantt.config.types.task) {
                return "<b>Priority: </b>" + task.priority;
            }

            return "";
        };

        gantt.attachEvent("onAfterLinkAdd", function (itemId, item) {

            if (!item.skipCreate) {

                gantt.changeLinkId(itemId, Math.uuid(9, 10));

                datacontext.createEntity("TaskLink", {
                    Id: item.id,
                    SourceId: item.source,
                    TargetId: item.target,
                    Type: item.type,
                    ProjectId: id
                });
            }

            updateScope();
        });

        gantt.attachEvent("onAfterLinkDelete", function (itemId, item) {

            var entity = getLinkEntity(itemId);

            if (entity != null) {
                entity.entityAspect.setDeleted();
                updateScope();
            }
        });

        gantt.attachEvent("onAfterTaskAdd", function (itemId, item) {

            if (!item.skipCreate) {

                gantt.changeTaskId(itemId, Math.uuid(9, 10));

                datacontext.createEntity("Task", {
                    Name: item.text,
                    StartDate: item.start_date,
                    Duration: item.duration,
                    ProjectId: id,
                    ParentTaskId: item.parent != 0 ? item.parent : null,
                    Id: item.id,
                    SortOrder: item.sort_order || 0,
                    Progress: item.progress,
                    Status: 'Planned',
                    Type: 'Task'
                });
            }

            updateScope();
        });


        gantt.attachEvent("onAfterTaskUpdate", function (itemId, item) {

            var entity = getTaskEntity(itemId);

            if (entity != null) {

                entity.Name = item.text;
                entity.StartDate = item.start_date;
                entity.Duration = item.duration;
                entity.SortOrder = item.sort_order || 0;
                entity.Progress = item.progress;

                updateScope();
            }
        });


        gantt.attachEvent("onAfterTaskDelete", function (itemId, item) {

            var entity = getTaskEntity(itemId);

            if (entity != null) {
                entity.entityAspect.setDeleted();
                updateScope();
            }
        });


        function getEntity(type, itemId) {
            var query = datacontext.createQuery()
                .from(type)
                .where('Id', '==', itemId);

            var data = datacontext.executeQueryLocally(query);
            return data.length > 0 ? data[0] : null;
        }

        function getTaskEntity(itemId) {
            return getEntity('Tasks', itemId);
        }

        function getLinkEntity(itemId) {
            return getEntity('TaskLinks', itemId);
        }

        var id = $state.params.id;
        loadTasks();

        function updateScope() {

            if (!$scope.$$phase) {
                $scope.$apply();
            }
        }

        console.log("ok!");

        function loadTasks() {

            $("#gantt").dhx_gantt({
                data: items,
                start_date: new Date(),
                scale_unit: "month",
                date_scale: "%F, %Y",
                scale_height: 50,
                subscales: [
                    { unit: "day", step: 1, date: "%j. %D" }
                ]
            });

            gantt.clearAll();

            var query = datacontext.createQuery()
                .from('Tasks')
                .where('ProjectId', '==', id)
                .where('ParentTaskId', '==', null)
                .expand('Subtasks,Subtasks.Subtasks,Subtasks.Subtasks.Subtasks,AssignedToUsers,Creator,Editor');

            datacontext.executeQuery(query, function (data) {
                updateTasks(data);
                loadLinks();
            }, function (error) {
                toastr.error(error.message, "Error!");
            });
        }

        function loadLinks() {

            var query = datacontext.createQuery()
                .from('TaskLinks')
                .where('ProjectId', '==', id);

            datacontext.executeQuery(query, function (data) {
                updateLinks(data);
            }, function (error) {
                toastr.error(error.message, "Error!");
            });
        }

        function updateTasks(data) {
            if (data.results.length == 0) {
                gantt.addTask({
                    id: Math.uuid(9, 10),
                    text: "Prosjekt",
                    start_date: new Date(),
                    type: gantt.config.types.project,
                    duration: 31
                });
            } else {
                traverse(data.results[0]);
            }
        }

        function traverse(item) {
            gantt.addTask({
                id: item.Id,
                text: item.Name,
                start_date: item.StartDate,
                duration: item.Duration,
                sort_order: item.SortOrder,
                open: true,
                progress: item.Progress,
                skipCreate: true,
                type: getGanttType(item.Type)
            }, item.ParentTaskId);

            _.each(item.Subtasks, traverse);
        }

        function updateLinks(data) {
            _.each(data.results, function (item) {
                gantt.addLink({
                    id: item.Id,
                    source: item.SourceId,
                    target: item.TargetId,
                    type: item.Type,
                    skipCreate: true
                });
            });
        }

        function updateButtonStates() {
            var selectedItems = getSelectedItems().length;

            _.each(toolbar, function (button) { button.visible = true; });

            if ($scope.isLoading) {
                _.each(toolbar, function (button) { button.disabled = true; });
            } else {
                toolbar.createButton.disabled = selectedItems.length > 1;
                toolbar.copyButton.disabled = !selectedItems;
                toolbar.deleteButton.disabled = !selectedItems;
                toolbar.resetButton.disabled = !selectedItems;
                toolbar.refreshButton.disabled = datacontext.hasNewItems();

                var items = clipboard.paste();
                /*
                toolbar.pasteButton.disabled = !(items && items.type == type) || !permissions.hasPermission(entityInfo.entityType, 'Create');
        
                toolbar.createButton.visible = permissions.hasPermission(entityInfo.entityType, 'Create');
                toolbar.copyButton.visible = permissions.hasPermission(entityInfo.entityType, 'Create');
                toolbar.pasteButton.visible = permissions.hasPermission(entityInfo.entityType, 'Create');
                toolbar.deleteButton.visible = permissions.hasPermission(entityInfo.entityType, 'Delete');*/
            }
        }


        function createNewTask() {

        }

        function copyTasks() {

        }


        function pasteTasks() {

        }


        function deleteTasks() {

        }


        function resetTasks() {

        }


        function refreshTasks() {

        }

    }
})();

function zoomTasks(value) {
    switch (value) {
        case "week":
            gantt.config.scale_unit = "day";
            gantt.config.date_scale = "%d %M";

            gantt.config.scale_height = 60;
            gantt.config.subscales = [
                  { unit: "hour", step: 6, date: "%H" }
            ];
            break;
        case "trplweek":
            gantt.config.scale_unit = "day";
            gantt.config.date_scale = "%d %M";
            gantt.config.subscales = [];
            gantt.config.scale_height = 35;
            break;
        case "month":
            gantt.config.scale_unit = "week";
            gantt.config.date_scale = "Week #%W";
            gantt.config.subscales = [
                  { unit: "day", step: 1, date: "%D" }
            ];

            gantt.config.scale_height = 60;
            break;
        case "year":
            gantt.config.scale_unit = "month";
            gantt.config.date_scale = "%M";
            gantt.config.scale_height = 60;
            gantt.config.subscales = [
                  { unit: "week", step: 1, date: "#%W" }
            ];
            break;
    }
    gantt.render();
}