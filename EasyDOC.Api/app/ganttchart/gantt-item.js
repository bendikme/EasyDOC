function GanttItem(item, parent) {

    var that = this;

    var SUMMARY_TASK = 0, TASK = 1, MILESTONE = 2;
    var LEFT_HANDLE = 0, RIGHT_HANDLE = 1, BODY = 2;

    var task = {};
    var strokeWidth = 11;
    var handleWidth = 4;
    var offsetY = 14;

    var critical = false;

    var calendar = new Calendar();

    item.gantt = _.extend(item.gantt, {
        type: 'task',
        item: this,
    });

    function getItemType() {
        return item.ChildTasks.length > 0 ? SUMMARY_TASK : item.IsMilestone ? MILESTONE : TASK;
    }

    function getFirstStartDate(currentItem, firstDate) {

        if (currentItem.ChildTasks.length == 0 && (firstDate == null || currentItem.StartDate < firstDate)) {
            return currentItem.StartDate;
        }

        _.each(currentItem.ChildTasks, function (i) {
            var date = getFirstStartDate(i, firstDate);
            firstDate = firstDate == null ? date : Math.min(firstDate, date);
        });

        return firstDate;
    }

    function getLastEndDate(currentItem, lastDate) {

        if (currentItem.ChildTasks.length == 0 && (lastDate == null || currentItem.EndDate > lastDate)) {
            return currentItem.EndDate;
        }

        _.each(currentItem.ChildTasks, function (i) {
            var date = getLastEndDate(i, lastDate);
            lastDate = lastDate == null ? date : Math.max(lastDate, date);
        });

        return lastDate;
    }

    function createSummaryTask() {

        var minX = parent.getX(moment(getFirstStartDate(item, null)).unix());
        var maxX = parent.getX(moment(getLastEndDate(item, null)).unix());

        var width = maxX - minX;

        task.graphic = new Kinetic.Group();

        task.body = new Kinetic.Rect({
            width: width,
            height: 3,
            fill: '#555'
        });

        task.leftBoundary = new Kinetic.Line({
            points: [0, 0, strokeWidth / 2, 0, 0, strokeWidth],
            fill: '#555',
            stroke: 0,
            closed: true
        });

        task.rightBoundary = new Kinetic.Line({
            points: [width, 0, width - strokeWidth / 2, 0, width, strokeWidth],
            fill: '#555',
            stroke: 0,
            closed: true
        });

        task.employees = new Kinetic.Text({
            y: -12,
            fill: 'black',
            fontStyle: 'bold',
            fontSize: 10,
            fontFamily: 'Open Sans',
            width: 300,
            padding: 0,
            align: 'left'
        });

        task.graphic.add(task.body);
        task.graphic.add(task.leftBoundary);
        task.graphic.add(task.rightBoundary);
        task.graphic.add(task.employees);
    }

    function createTask() {

        var params = parent.getParams();

        task.graphic = new Kinetic.Group();

        task.employees = new Kinetic.Text({
            y: 14,
            fill: 'black',
            fontSize: 10,
            fontFamily: 'Open Sans',
            width: 300,
            padding: 0,
            align: 'left'
        });

        task.leftDependencyHandle = new Kinetic.Circle({
            y: 8,
            radius: 4,
            fill: '#eee',
            stroke: '#555',
            strokeWidth: 0.5,
            visible: false
        });

        task.rightDependencyHandle = new Kinetic.Circle({
            y: 8,
            radius: 4,
            fill: '#eee',
            stroke: '#555',
            strokeWidth: 0.5,
            visible: false
        });

        if (getItemType() == TASK) {
            task.body = new Kinetic.Rect({
                height: params.lineHeight,
                fill: item.Color,
                stroke: 'transparent',
                strokeWidth: 1
            });
        } else {
            task.body = new Kinetic.Rect({
                width: 8,
                height: 8,
                fill: 'black',
                rotation: 45,
                strokeWidth: 1
            });
        }

        task.bodyOutline = new Kinetic.Rect({
            height: params.lineHeight,
            fill: 'transparent',
            draggable: true,
            stroke: 'transparent',
            strokeWidth: 2,
            dragBoundFunc: function (pos) {
                return parent.snapToGrid.apply(this, [pos, 0, { handle: BODY, item: item }]);
            }
        });

        task.leftHandle = new Kinetic.Rect({
            width: handleWidth,
            height: params.lineHeight,
            fill: '#555',
            stroke: '#333',
            strokeWidth: 1,
            draggable: true,
            dragBoundFunc: function (pos) {
                return parent.snapToGrid.apply(this, [pos, handleWidth, { handle: LEFT_HANDLE, item: item }]);
            },
            visible: false
        });

        task.rightHandle = new Kinetic.Rect({
            width: handleWidth,
            height: params.lineHeight,
            fill: '#555',
            stroke: '#333',
            strokeWidth: 1,
            draggable: true,
            dragBoundFunc: function (pos) {
                return parent.snapToGrid.apply(this, [pos, handleWidth, { handle: RIGHT_HANDLE, item: item }]);
            },
            visible: false
        });

        task.progressBar = new Kinetic.Rect({
            height: params.lineHeight,
            opacity: 0.5
        });

        task.graphic.add(task.leftDependencyHandle);
        task.graphic.add(task.rightDependencyHandle);
        task.graphic.add(task.body);
        task.graphic.add(task.progressBar);
        task.graphic.add(task.bodyOutline);
        task.graphic.add(task.employees);
        task.graphic.add(task.leftHandle);
        task.graphic.add(task.rightHandle);
        task.graphic.add(task.leftDependencyHandle);
        task.graphic.add(task.rightDependencyHandle);

        setCursorOnMouseOver(task.leftHandle, 'ew-resize');
        setCursorOnMouseOver(task.rightHandle, 'ew-resize');
        setCursorOnMouseOver(task.bodyOutline, 'move');

        task.leftDependencyHandle.on('mousedown', function () { parent.startDrawLine(item, task, LEFT_HANDLE); });
        task.rightDependencyHandle.on('mousedown', function () { parent.startDrawLine(item, task, RIGHT_HANDLE); });

        task.leftDependencyHandle.on('mouseover', function () { onMouseOverDependencyHandle(LEFT_HANDLE); });
        task.rightDependencyHandle.on('mouseover', function () { onMouseOverDependencyHandle(RIGHT_HANDLE); });

        task.leftDependencyHandle.on('mouseup', function () { onMouseUpDependencyHandle(LEFT_HANDLE); });
        task.rightDependencyHandle.on('mouseup', function () { onMouseUpDependencyHandle(RIGHT_HANDLE); });

        task.leftDependencyHandle.on('mouseout', function () { onMouseOutDependencyHandle(LEFT_HANDLE); });
        task.rightDependencyHandle.on('mouseout', function () { onMouseOutDependencyHandle(RIGHT_HANDLE); });

        task.leftHandle.on('mouseover', onMouseOverHandle);
        task.rightHandle.on('mouseover', onMouseOverHandle);

        task.leftHandle.on('mouseout', onMouseOutHandle);
        task.rightHandle.on('mouseout', onMouseOutHandle);

        var dragStartState;

        task.leftHandle.on('dragstart', function () {
            dragStartState = storeDragStartState(LEFT_HANDLE);
            document.body.style.cursor = 'ew-resize';
            task.bodyOutline.stroke('black');
            hideHandles();
        });

        task.rightHandle.on('dragstart', function () {
            dragStartState = storeDragStartState(RIGHT_HANDLE);
            document.body.style.cursor = 'ew-resize';
            task.bodyOutline.stroke('black');
            hideHandles();
        });

        task.bodyOutline.on('dragstart', function () {
            dragStartState = storeDragStartState(BODY);
            task.bodyOutline.stroke('black');
            hideHandles();
        });

        task.bodyOutline.on('mouseover', function () {
            if (!item.IsMilestone) {
                showHandles();
            }
        });

        task.bodyOutline.on('click', function () {
            that.select();
            parent.invalidate();
            fireEvent('selectionChanged', that);
        });

        task.bodyOutline.on('mouseout', function () {
            delayHideHandles();
        });

        var lastLeftDragMoveX;
        var lastRightDragMoveX;
        var lastBodyDragMove;

        task.leftHandle.on('dragmove', function (event) {

            var newX = event.target.attrs.x + handleWidth;
            if (lastLeftDragMoveX == newX) return;
            lastLeftDragMoveX = newX;

            var oldX = task.bodyOutline.getX();
            var width = task.bodyOutline.getWidth() - (newX - oldX);

            item.newStartDate = parent.snapDate(moment.unix(parent.getSeconds(newX)), LEFT_HANDLE).toDate();
            item.newEndDate = item.EndDate;

            task.bodyOutline.setX(newX);
            task.bodyOutline.setWidth(width);

        });

        task.rightHandle.on('dragmove', function (event) {

            var endX = event.target.attrs.x;
            if (lastRightDragMoveX == endX) return;
            lastRightDragMoveX = endX;

            var startX = task.bodyOutline.getX();
            var width = endX - startX;

            item.newStartDate = item.StartDate;
            item.newEndDate = parent.snapDate(moment.unix(parent.getSeconds(endX)), RIGHT_HANDLE).toDate();

            task.bodyOutline.setX(startX);
            task.bodyOutline.setWidth(width);
        });

        task.bodyOutline.on('dragmove', function (event) {

            var startX = event.target.attrs.x;
            if (lastBodyDragMove == startX) return;

            var start = calendar.skipOfftime(parent.snapDate(moment.unix(parent.getSeconds(startX)), LEFT_HANDLE), 1);
            var end = calendar.add(moment(start), item.Duration, true);
            item.newStartDate = start.toDate();
            item.newEndDate = end.toDate();

            startX = parent.getX(start.unix());
            var endX = parent.getX(end.unix());

            task.bodyOutline.setX(startX);
            task.bodyOutline.setWidth(endX - startX);

            lastBodyDragMove = startX;
            parent.invalidate();
        });

        task.leftHandle.on('dragend', function () { onDragEndHandle(LEFT_HANDLE); });
        task.rightHandle.on('dragend', function () { onDragEndHandle(RIGHT_HANDLE); });
        task.bodyOutline.on('dragend', function () { onDragEndHandle(BODY); });
    }

    this.delete = function () {
        if (getItemType() == SUMMARY_TASK) {
            task.graphic.remove();
        } else {
            task.graphic.remove();

            _.each(_.union(item.Successors, item.Predecessors), function (t) {
                t.gantt.item.delete();
                t.entityAspect.setDeleted();
            });

            item.ParentTask.gantt.item.update();
        }
    }

    this.deselect = function () {
        if (item.isSelected) {
            item.isSelected = false;
        }
    };

    this.select = function () {
        item.isSelected = !item.isSelected;
    }

    this.setCritical = function (isCritical) {
        critical = isCritical;
        item.IsCritical = critical;
    }

    this.isCritical = function () {
        return critical;
    }

    var callbacks = {
        changed: [],
        dragged: [],
        selectionChanged: [],
    };


    function fireEvent(event, args) {
        _.each(callbacks[event], function (func) {
            func(item, args);
        });
    }

    this.on = function (event, callback) {
        if (_.isArray(callbacks[event]) && _.isFunction(callback)) {
            callbacks[event].push(callback);
        }
    };

    this.update = function (x1, x2) {

        var offsetX = 0;
        var type = getItemType();

        var params = parent.getParams();

        switch (type) {

            case MILESTONE:
                offsetX = -2;

            case TASK:

                var startX = Math.round(x1 || parent.getX(moment(item.StartDate).unix()) + offsetX) + 0.5;
                var endX = Math.round(x2 || parent.getX(moment(item.EndDate).unix()) + offsetX) + 0.5;

                var width = endX - startX;

                task.graphic.setY(item.gantt.position * params.rowHeight + 2);
                task.body.setX(startX);
                task.bodyOutline.setX(startX);

                if (type == MILESTONE) {

                    var milestoneHeight = params.lineHeight / 2 + 3;

                    task.body.setHeight(milestoneHeight);
                    task.body.setWidth(milestoneHeight);
                    task.body.fill('#636363');
                    task.body.rotation(45);

                    task.bodyOutline.setHeight(milestoneHeight);
                    task.bodyOutline.setWidth(milestoneHeight);
                    task.bodyOutline.rotation(45);

                    task.leftDependencyHandle.setX(startX - 12);
                    task.rightDependencyHandle.setX(endX + handleWidth + 8);

                    task.progressBar.visible(false);
                    task.employees.visible(false);

                } else {

                    task.body.setHeight(params.lineHeight);
                    task.body.setWidth(width);
                    task.body.fill(critical ? '#f78a88' : '#8abbed');
                    task.body.stroke(critical ? '#dd2421' : '#3b87d4');
                    task.body.rotation(0);

                    task.bodyOutline.setHeight(params.lineHeight);
                    task.bodyOutline.setWidth(width);
                    task.bodyOutline.rotation(0);

                    task.progressBar.setX(startX);
                    task.progressBar.setWidth(width * item.PercentComplete / 100);
                    task.progressBar.fill(critical ? '#dd2421' : '#3b87d4');

                    task.leftDependencyHandle.setX(startX - handleWidth * 2);
                    task.rightDependencyHandle.setX(endX + handleWidth * 2);

                    task.employees.setX(startX);
                    task.employees.text(getEmployeeText());

                    task.leftHandle.setX(startX - handleWidth);
                    task.rightHandle.setX(endX);

                    task.progressBar.visible(true);
                    task.employees.visible(true);
                }

                break;

            case SUMMARY_TASK:

                var startDate = getFirstStartDate(item, null);
                var endDate = getLastEndDate(item, null);

                startX = parent.getX(moment(startDate).unix());
                endX = parent.getX(moment(endDate).unix());

                width = endX - startX;

                task.employees.text(getEmployeeText());

                task.graphic.setX(startX);
                task.graphic.setY(offsetY + item.gantt.position * params.rowHeight + 2);
                task.body.setWidth(endX - startX);
                task.rightBoundary.setPoints([width, 0, width - strokeWidth / 2, 0, width, strokeWidth]);

                item.StartDate = startDate;
                item.EndDate = endDate;

                fireEvent('changed');
                break;

        }
    }

    this.draw = function () {

        if (task.graphic) {
            task.graphic.destroy();
            task = {};
        }

        switch (getItemType()) {

            case SUMMARY_TASK:
                createSummaryTask();
                return this;

            case TASK:
            case MILESTONE:
                createTask();
                return this;

            default:
                return this;
        }

    };

    this.getComponent = function () {
        return task.graphic;
    };

    function setCursorOnMouseOver(graphic, cursor) {
        graphic.on('mouseover', function () {
            document.body.style.cursor = cursor;
        });
        graphic.on('mouseout', function () {
            document.body.style.cursor = 'default';
        });
    }

    function getEmployeeText() {

        return item.Name;

        return item.Resources.map(function (relation) {
            return relation.Employee.Name;
        }).join(', ');
    }

    function storeDragStartState(handle) {

        if (handle == BODY) return null;

        function getProperties(part) {
            return {
                width: part.getWidth(),
                x: part.getX()
            }
        };

        return {
            body: getProperties(task.body),
            leftHandle: getProperties(task.leftHandle),
            rightHandle: getProperties(task.rightHandle),
            handle: handle
        }
    }

    function onMouseOverDependencyHandle(handle) {
        var h = handle == LEFT_HANDLE ? task.leftDependencyHandle : task.rightDependencyHandle;
        showHandles();
        h.fill('#aaa').draw();
    }

    function onMouseUpDependencyHandle(handle) {
        if (parent.isDrawingLine() && item != parent.getLineSourceItem()) {
            parent.createDependency(item, handle);
        }
    }

    function onMouseOutDependencyHandle() {
        delayHideHandles();
    }

    function onMouseOverHandle() {
        showHandles();
    }

    function onMouseOutHandle() {
        delayHideHandles();
    }

    function onDragEndHandle(handle) {
        task.bodyOutline.stroke('transparent');
        that.update();
        hideHandles();

        fireEvent('dragged', handle);
    }

    var hideHandleTimer;

    function showHandles() {
        clearTimeout(hideHandleTimer);
        task.leftDependencyHandle.visible(true);
        task.rightDependencyHandle.visible(true);
        task.leftHandle.visible(true);
        task.rightHandle.visible(true);
        parent.invalidate();
    }

    function delayHideHandles() {
        task.leftDependencyHandle.fill('#eee');
        task.rightDependencyHandle.fill('#eee');
        parent.invalidate();
        hideHandleTimer = setTimeout(hideHandles, 100);
    }

    function hideHandles() {
        task.leftDependencyHandle.fill('#eee');
        task.rightDependencyHandle.fill('#eee');
        task.leftDependencyHandle.visible(false);
        task.rightDependencyHandle.visible(false);
        task.leftHandle.visible(false);
        task.rightHandle.visible(false);
        parent.invalidate();
    }

}