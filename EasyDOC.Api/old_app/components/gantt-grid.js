function GanttChart(params) {

    var LEFT_HANDLE = 0, RIGHT_HANDLE = 1, BODY = 2;

    var that = this;
    var sourceTask = null;
    var gridStartSeconds = params.start.hour(0).minute(0).second(0).millisecond(0).unix();
    var gridEndSeconds = params.end.hour(0).minute(0).second(0).milliseconds(0).unix();

    var timespan = gridEndSeconds - gridStartSeconds;
    var convertValue;
    var snapValue;
    var subWidthValue;

    var calendar = new Calendar();

    var projectStartDate = moment(params.projectStartDate).hour(8).toDate();

    var mainWidth;
    var subWidth;
    var divisions;

    function setZoomParameters() {

        gridStartSeconds = params.start.hour(0).minute(0).second(0).millisecond(0).unix();
        gridEndSeconds = params.end.hour(8).minute(0).second(0).milliseconds(0).unix();
        timespan = gridEndSeconds - gridStartSeconds;

        params.canvasWidth = params.subWidth * params.subDivisions;

        if (chart.getHeight() != params.height) {
            chart.setHeight(params.height);
        }

        switch (params.zoomLevel) {

            case 'day':
                convertValue = 3600 * 24;
                snapValue = 3600;
                subWidthValue = snapValue * 2;
                break;

            case 'week':
                convertValue = 3600 * 24 * 7;
                snapValue = 3600 * 6;
                subWidthValue = 3600 * 24;
                break;

            case 'month':
                convertValue = 3600 * 24 * 365 / 12;
                snapValue = 3600 * 24 * 365 / 52;
                subWidthValue = snapValue;
                break;
        }


        var normal = timespan / convertValue;
        divisions = timespan / convertValue;
        mainWidth = params.canvasWidth / divisions;

        normal = timespan / subWidthValue;
        subWidth = params.canvasWidth / normal;
    }

    var taskCollection = {};
    var dependencyCollection = {};

    var callbacks = {
        dependencyCreated: [],
        taskChanged: [],
        selectionChanged: [],
    };

    function fireEvent(event, args) {
        _.each(callbacks[event], function (func) {
            func(args);
        });
    }

    this.on = function (event, callback) {
        if (_.isArray(callbacks[event]) && _.isFunction(callback)) {
            callbacks[event].push(callback);
        }
    };

    var header = new Kinetic.Stage({
        container: params.header,
        width: params.subWidth * params.subDivisions,
        height: params.headerHeight
    });

    var chart = createStage(params.container);
    var timescale = new Kinetic.Layer();
    var gridLayer = new Kinetic.Layer();
    var tasks = new Kinetic.Layer();
    var dependencies = new Kinetic.Layer();
    var preview = new Kinetic.Layer();

    header.add(timescale);

    chart.add(gridLayer);
    chart.add(dependencies);
    chart.add(tasks);
    chart.add(preview);

    var previewLine = new Kinetic.Line({
        dash: [5, 5],
        stroke: '#555',
        strokeWidth: 2,
        opacity: 0.5,
        visible: false
    });

    preview.add(previewLine);

    function deselectTasks(exception) {
        _.each(taskCollection, function (task) {
            if (task.gantt.item != exception) {
                task.gantt.item.deselect();
            }
        });
    }

    function deselectDependencies(exception) {
        _.each(_.values(dependencyCollection), function (dependency) {
            if (dependency != exception) {
                dependency.deselect();
            }
        });
    }

    function calculateTaskLength(task) {
        task.cpm.lengthCalculated = true;
        return calendar.getDuration(moment(task.StartDate), moment(task.EndDate), true);
    }

    function onTaskChanged(task) {

        fireEvent('taskChanged', arguments);

        if (task.ParentTask != null && task.ParentTask.cpm && !task.ParentTask.cpm.lengthCalculated) {
            task.ParentTask.gantt.item.update();
            task.ParentTask.Duration = calculateTaskLength(task.ParentTask);
        }
    }

    function onTaskDragged(task, handle) {
        switch (handle) {

            case LEFT_HANDLE:
                // Set a 'Start no earlier than'-constraint and change duration
                var start = moment(new Date(task.newStartDate));

                var end = moment(task.EndDate);
                var seconds = calendar.getDifferenceSeconds(start, end);

                task.ConstraintDate = task.newStartDate;
                task.ConstraintType = 'StartNoEarlierThan';
                task.Duration = (seconds / 28800) + 'd';
                that.rescheduleTasks();
                break;

            case RIGHT_HANDLE:
                // Change the duration of the task. Snap to nearest workday.
                start = moment(new Date(task.StartDate));
                end = moment(task.newEndDate);
                seconds = calendar.getDifferenceSeconds(start, end);

                task.Duration = (seconds / 28800) + 'd';
                that.rescheduleTasks();
                break;

            case BODY:
                // Set a 'Start no earlier than'-constraint
                var m = moment(task.newStartDate).hour(0);
                calendar.skipOfftime(m, 1, true);
                task.ConstraintDate = m.toDate();
                task.ConstraintType = 'StartNoEarlierThan';
                that.rescheduleTasks();
                break;
        }
    }

    this.deselectAll = function () {
        deselectTasks();
        deselectDependencies();
    }

    this.redrawTask = function (task) {
        task.gantt.item.draw();
        var component = task.gantt.item.getComponent();
        tasks.add(component);
    }

    this.addTask = function (item) {

        if (item.ParentTask != null && item.ParentTask.ChildTasks.length == 1) {
            that.redrawTask(item.ParentTask);
        }

        var task = new GanttItem(item, that);
        task.on('changed', onTaskChanged);
        task.on('dragged', onTaskDragged);

        task.on('selectionChanged', function (i, t) {
            deselectTasks(t);
            deselectDependencies();
            fireEvent('selectionChanged', item);
        });

        task.draw();
        taskCollection[item.Id] = item;

        component = task.getComponent();
        tasks.add(component);
        task.update();
    };

    this.addDependency = function (item) {
        var dependency = new GanttDependency(item, that);
        storeDependency(item, dependency);
        drawDependency(dependency);

        dependency.on('selectionChanged', function (i, d) {
            deselectDependencies(d);
            deselectTasks();
            fireEvent('selectionChanged', item);
        });
    };

    function drawDependency(dependency) {
        var component = dependency.draw();
        dependencies.add(component);
    }

    this.delete = function (item) {
        if (item.gantt.type == 'task') {
            delete (taskCollection[item.Id]);
        } else {
            removeDependency(item);
        }

        item.gantt.item.delete();
        this.invalidate();
    }

    this.createDependency = function (targetTask, targetHandle) {
        fireEvent('dependencyCreated', {
            source: sourceTask.item,
            sourceHandle: sourceTask.handle,
            target: targetTask,
            targetHandle: targetHandle
        });

        this.rescheduleTasks();
    };

    this.startDrawLine = function (sourceItem, component, handle) {

        var property = handle == LEFT_HANDLE ? 'leftDependencyHandle' : 'rightDependencyHandle';

        sourceTask = {
            lineX: Math.floor(component[property].getX()) + 0.5,
            lineY: Math.floor(component.graphic.getY()) + 6.5,
            item: sourceItem,
            component: component,
            handle: handle
        };

        previewLine.visible(true);
    };

    this.stopDrawLine = function () {
        sourceTask = null;
        previewLine.visible(false);
    };

    this.getLineSourceItem = function () {
        return sourceTask.item;
    };

    this.isDrawingLine = function () {
        return sourceTask != null;
    };

    this.getSeconds = function (x) {
        return gridStartSeconds + ((x / mainWidth) * convertValue);
    };

    this.getX = function (seconds) {
        var date = moment.unix(seconds);
        if (params.zoomLevel != 'day') {
            if (date.hour() <= 8) {
                date.hour(0).minute(0).second(0).millisecond(0);
            } else if (date.hour() >= 16) {
                date.hour(0).minute(0).second(0).millisecond(0).add('day', 1);
            }
        }

        seconds = date.unix();
        return ((seconds - gridStartSeconds) / convertValue) * mainWidth;
    };

    this.snapToGrid = function (pos, offset, settings) {

        var snapSettings = sourceTask || settings;

        offset = _.isNumber(offset) ? offset : 0;

        var seconds = that.getSeconds(pos.x);
        var date = that.snapDate(moment.unix(seconds));

        if (snapSettings && snapSettings.handle == LEFT_HANDLE && date >= snapSettings.item.newEndDate) {
            return {
                x: that.getX(moment(snapSettings.item.newStartDate).unix()) - offset,
                y: this.getAbsolutePosition().y
            }
        } else if (snapSettings && snapSettings.handle == RIGHT_HANDLE && date <= snapSettings.item.newStartDate) {
            return {
                x: that.getX(moment(snapSettings.item.newEndDate).unix()) - offset,
                y: this.getAbsolutePosition().y
            }
        }

        return {
            x: that.getX(date.unix()) - offset,
            y: this.getAbsolutePosition().y
        }
    };

    this.snapDate = function (date, handle) {
        switch (params.zoomLevel) {
            case 'day':
                date = date.minutes() < 30 ? date : date.add('minute', 30);
                return date.minute(0).second(0).millisecond(0);
            case 'week':
            case 'month':
                date = date.hour() < 12 ? date : date.add('hour', 12);
                date.hour(0).minute(0).second(0).millisecond(0);

                if (handle == LEFT_HANDLE) {
                    date.hour(8);
                } else if (handle == RIGHT_HANDLE) {
                    date.subtract('day', 1).hour(16);
                }

            default:
                return date;
        }
    };

    this.drawGrid = function (gridParams) {

        params = _.extend(params, gridParams);
        setZoomParameters();

        var minSubWidth = 30;
        var fontSize = 13;
        var subFontSize = Math.min(Math.max(subWidth, minSubWidth) / 1.2, 9);
        var fontFamily = 'Segoe UI';

        timescale.destroyChildren();
        gridLayer.destroyChildren();

        var grid = new Kinetic.Group();

        var bg = new Kinetic.Rect({
            width: params.canvasWidth,
            height: params.height,
            fill: 'white'
        });

        grid.add(bg);

        bg.on('click', function () {
            deselectDependencies();
            deselectTasks();
            fireEvent('selectionChanged');
        });

        var header = new Kinetic.Rect({
            width: params.canvasWidth,
            height: params.headerHeight,
            fill: '#428BCA'
        });

        timescale.add(header);

        var lastX = -1000;
        var realWidth = Math.max(subWidth, minSubWidth);

        var x = 0;
        var date = moment.unix(this.getSeconds(0));
        date.isoWeekday(1).hours(0).minutes(0).seconds(0).milliseconds(0);

        var widthAdjust = 1;

        for (var i = this.getX(date.unix()) ; i <= timespan && x < params.canvasWidth; i += subWidth) {

            x = Math.floor(i) + 0.5;
            if (x < -realWidth || x - lastX < minSubWidth) {
                ++widthAdjust;
                continue;
            } else {
                widthAdjust = 1;
                lastX = x;
            }

            date = moment.unix(this.getSeconds(i));

            horizontalGridLine = new Kinetic.Line({
                points: [x, params.mainHeaderHeight, x, params.headerHeight],
                stroke: 'white',
                strokeWidth: 1
            });

            var textWidth = realWidth * widthAdjust;
            var name = getSubLevelText(date, textWidth);

            var text = new Kinetic.Text({
                x: x,
                y: params.mainHeaderHeight + subFontSize / 2,
                text: name,
                fontSize: subFontSize,
                fontFamily: fontFamily,
                fill: 'white',
                width: textWidth,
                padding: 0,
                align: 'center'
            });

            if (name[0] == 'S') {

                var rect = new Kinetic.Rect({
                    x: x,
                    y: 0,
                    width: subWidth - 2,
                    height: chart.getHeight(),
                    fill: '#eee',
                    opacity: 0.4,
                    strokeWidth: 0
                });

                grid.add(rect);
            }

            timescale.add(horizontalGridLine);
            timescale.add(text);
        }

        x = 0;
        date = moment.unix(this.getSeconds(0));

        widthAdjust = 1;
        lastX = -1000;
        realWidth = Math.max(mainWidth, 100);

        for (i = this.getX(date.unix()) ; i <= timespan && x < params.canvasWidth; i += mainWidth) {

            x = Math.floor(i) + 0.5;
            if (x < -realWidth || x - lastX < 100) {
                ++widthAdjust;
                continue;
            } else {
                widthAdjust = 1;
                lastX = x;
            }

            date = moment.unix(this.getSeconds(i));

            text = new Kinetic.Text({
                x: x,
                y: fontSize / 2,
                text: getMainLevelText(date),
                fontSize: fontSize,
                fontFamily: fontFamily,
                fill: 'white',
                width: realWidth * widthAdjust,
                padding: 2,
                align: 'left'
            });

            var horizontalGridLine = new Kinetic.Line({
                points: [x, 0, x, params.headerHeight],
                stroke: 'white',
                strokeWidth: 1,
                dash: [1, 2]
            });

            timescale.add(text);
            timescale.add(horizontalGridLine);
        }

        gridLayer.add(grid);
        gridLayer.draw();
        timescale.draw();
    }

    this.getParams = function () {
        return params;
    };

    this.invalidate = function () {
        console.log('invalidated');
        tasks.batchDraw();
        dependencies.batchDraw();
    };

    function markAllTasks(task, graphId) {

        if (task.cpm.graphId > -1) return;

        task.cpm.graphId = graphId;

        _.each(task.Predecessors, function (relation) {
            markAllTasks(relation.From, graphId);
        });

        _.each(task.Successors, function (relation) {
            markAllTasks(relation.To, graphId);
        });
    }

    this.rescheduleTasks = function () {

        // Initialize the needed critial path method-data structure on all task instances
        _.each(taskCollection, function (task) {
            if (task.ChildTasks.length == 0) {
                task.cpm = {
                    graphId: -1,
                    id: -1,
                    duration: getDuration(task.Duration),
                    actualStartTime: -1,
                    earlyStartTime: 0,
                    earlyFinishTime: 0,
                    lateStartTime: 0,
                    lateFinishTime: 0,
                    startFloat: 0,
                    finishFloat: 0
                };
            } else {
                task.cpm = {
                    lengthCalculated: false
                }
            }


            if (task.IsMilestone) {
                task.Duration = '0d';
            } else if (task.ChildTasks.length == 0) {
                var duration = calendar.parseDuration(task.Duration);
                if (duration.value == 0) {
                    task.Duration = '0d';
                    task.IsMilestone = true;
                }
            }

        });

        // Create two collections containing all the starting tasks and all the end tasks
        var startTasks = _.filter(taskCollection, function (task) {
            return task.ChildTasks.length == 0 && task.Predecessors.length == 0;
        });

        // Find unconnected graphs by traversing all nodes, marking them with an with an id
        var graphId = 0;
        _.each(startTasks, function (task) {
            if (task.cpm.graphId == -1) {
                markAllTasks(task, graphId);
                ++graphId;
            }
        });

        // Perform the critical path method forward pass on each of the graphs separately
        for (var i = 0; i < graphId; ++i) {

            var graphTasks = _.filter(taskCollection, function (task) { return task.cpm && task.cpm.graphId == i; });
            var id = 0;

            while (true) {
                var tasksReadyToBeMarked = _.filter(graphTasks, function (task) {
                    return task.cpm.id == -1 && (task.Predecessors.length == 0 || _.every(task.Predecessors, function (relation) {
                        return relation.From.cpm.id != -1;
                    }));
                });

                if (tasksReadyToBeMarked.length == 0) break;

                _.each(tasksReadyToBeMarked, function (readyTask) {
                    readyTask.cpm.id = id++;
                });
            }

            criticalPathMethodForwardPass(_.sortBy(graphTasks, function (task) {
                return task.cpm.id;
            }));
        }

        // Find the task that has the latest ending time. This will be used
        // as the project ending time.
        var lastTask = _.max(taskCollection, function (task) {
            return task.cpm && task.cpm.earlyFinishTime;
        });

        if (!lastTask.cpm) return;
        var projectFinishTime = lastTask.cpm.earlyFinishTime;

        // Now run the backward pass using the project finish time as a constraint
        // Finally update the tasks converting the time unit into actual dates and time
        for (i = 0; i < graphId; ++i) {
            graphTasks = _.filter(taskCollection, function (task) { return task.cpm && task.cpm.graphId == i; });
            var sortedBackwards = _.sortBy(graphTasks, function (task) { return -task.cpm.id; });
            criticalPathMethodBackwardPass(sortedBackwards, projectFinishTime);
            rescheduleTask(sortedBackwards);
        }

        updateDependencies();
        this.invalidate();
    };

    function isInflexible(task) {
        return ['MustStartOn', 'MustFinishOn'].indexOf(task.ConstraintType) > -1;
    }

    function rescheduleTask(sortedTasks) {

        _.each(sortedTasks, function (task) {

            var start = moment(new Date(projectStartDate));

            switch (task.ConstraintType) {

                case 'AsSoonAsPossible':
                case 'StartNoEarlierThan':
                case 'FinishNoEarlierThan':
                    task.cpm.actualStartTime = task.cpm.earlyStartTime;
                    break;

                case 'AsLateAsPossible':
                case 'StartNoLaterThan':
                case 'FinishNoLaterThan':
                    task.cpm.actualStartTime = task.cpm.lateStartTime;
                    _.each(task.Successors, function (rel) {
                        task.cpm.actualStartTime = Math.min(task.cpm.actualStartTime, rel.To.cpm.actualStartTime - task.cpm.duration);
                    });
                    break;

                case 'MustStartOn':
                case 'MustFinishOn':
                    task.cpm.actualStartTime = task.cpm.lateStartTime;
                    break;
            }

            calendar.add(start, task.cpm.actualStartTime / 3600 + 'h', true);
            calendar.skipOfftime(start, 1);

            task.StartDate = new Date(start.toDate());
            task.EndDate = calendar.add(start, task.Duration, true).toDate();

            task.gantt.item.update();
            onTaskChanged(task);
        });
    }

    function getDuration(duration) {
        var projectStart = moment(new Date(projectStartDate));
        var durationDate = moment(new Date(projectStartDate));
        calendar.add(durationDate, duration || '0d', false);

        return durationDate.diff(projectStart) / 1000;
    }

    function criticalPathMethodForwardPass(sortedTasks) {

        // Forward pass
        _.each(sortedTasks, function (task) {

            var maxTime = 0;

            _.each(task.Predecessors, function (rel) {

                var relType = rel.Type.split('To');
                var predecessorTime = relType[0] == 'Finish' ? (rel.From.cpm.earlyFinishTime || 0) : (rel.From.cpm.earlyStartTime || 0);
                var duration = relType[1] == 'Finish' ? task.cpm.duration : 0;

                maxTime = Math.max(maxTime, predecessorTime + getDuration(rel.Lag) - duration);
            });

            if (task.ConstraintType == 'StartNoEarlierThan') {
                maxTime = Math.max(maxTime, getConstraintTime(task));
            } else if (task.ConstraintType == 'FinishNoEarlierThan') {
                maxTime = Math.max(maxTime, getConstraintTime(task) - task.cpm.duration);
            }

            if (!isInflexible(task)) {
                task.cpm.earlyStartTime = maxTime;
                task.cpm.earlyFinishTime = task.cpm.earlyStartTime + task.cpm.duration;
            } else if (task.ConstraintType == 'MustStartOn') {
                task.cpm.earlyStartTime = getConstraintTime(task);
                task.cpm.earlyFinishTime = task.cpm.earlyStartTime + task.cpm.duration;
            } else if (task.ConstraintType == 'MustFinishOn') {
                task.cpm.earlyFinishTime = getConstraintTime(task);
                task.cpm.earlyStartTime = task.cpm.earlyFinishTime - task.cpm.duration;
            }
        });
    }

    function getConstraintTime(task) {
        return calendar.getDifferenceSeconds(moment(new Date(projectStartDate)), moment(task.ConstraintDate));
    }

    function criticalPathMethodBackwardPass(sortedTasks, projectFinishTime) {

        // Backward pass
        _.each(sortedTasks, function (task) {

            var minTime = projectFinishTime;

            _.each(task.Successors, function (rel) {

                var relType = rel.Type.split('To');
                var successorTime = relType[1] == 'Start' ? (rel.To.cpm.lateStartTime || 0) : (rel.To.cpm.lateFinishTime || 0);
                var duration = relType[0] == 'Start' ? task.cpm.duration : 0;

                minTime = Math.min(minTime, successorTime - getDuration(rel.Lag) + duration);
            });

            if (task.ConstraintType == 'FinishNoLaterThan') {
                minTime = Math.min(minTime, getConstraintTime(task));
            } else if (task.ConstraintType == 'StartNoLaterThan') {
                minTime = Math.min(minTime, getConstraintTime(task) + task.cpm.duration);
            }

            if (!isInflexible(task)) {
                task.cpm.lateFinishTime = minTime;
                task.cpm.lateStartTime = task.cpm.lateFinishTime - task.cpm.duration;
            } else if (task.ConstraintType == 'MustStartOn') {
                task.cpm.lateStartTime = getConstraintTime(task);
                task.cpm.lateFinishTime = task.cpm.lateStartTime + task.cpm.duration;
            } else if (task.ConstraintType == 'MustFinishOn') {
                task.cpm.lateFinishTime = getConstraintTime(task);
                task.cpm.lateStartTime = task.cpm.lateFinishTime - task.cpm.duration;
            }

            task.cpm.startFloat = task.cpm.lateStartTime - task.cpm.earlyStartTime;
            task.cpm.finishFloat = task.cpm.lateFinishTime - task.cpm.earlyFinishTime;

            task.gantt.item.setCritical((task.cpm.startFloat == 0 && task.cpm.finishFloat == 0) || (['AsLateAsPossible', 'MustStartOn', 'MustFinishOn'].indexOf(task.ConstraintType) > -1));

        });
    }

    function getMainLevelText(date) {
        switch (params.zoomLevel) {
            case 'day': return date.format('DD MMM \'YY');
            case 'week': return date.format('DD MMM \'YY (w)');
            case 'month': return date.format('MMMM \'YY');
            default: return '';
        }
    }

    function getSubLevelText(date, width) {
        switch (params.zoomLevel) {
            case 'day': return date.format('HH');
            case 'week': return width > 40 ? date.format('ddd') : date.format('dd')[0];
            case 'month': return date.format('w');
            default: return '';
        }
    }

    function createStage(container) {

        var ganttStage = new Kinetic.Stage({
            container: container,
            width: params.subWidth * params.subDivisions,
            height: params.height
        });

        ganttStage.on('mousemove', function (event) {
            if (sourceTask != null) {
                previewLine.setPoints([sourceTask.lineX, sourceTask.lineY, event.evt.offsetX, event.evt.offsetY]);
                preview.drawScene();
            }
        });

        ganttStage.on('mouseup', function () {
            document.body.style.cursor = 'default';
            sourceTask = null;
            previewLine.visible(false);
            preview.drawScene();
        });

        return ganttStage;
    }

    function storeDependency(item, dependency) {
        var key = item.FromId + ':::' + item.ToId + ':::' + item.Type;
        dependencyCollection[key] = dependency;
    }

    function removeDependency(item) {
        var key = item.FromId + ':::' + item.ToId + ':::' + item.Type;
        delete (dependencyCollection[key]);
    }

    function updateDependencies() {
        _.each(dependencyCollection, function (value, key) {
            drawDependency(dependencyCollection[key]);
        });
    }
}