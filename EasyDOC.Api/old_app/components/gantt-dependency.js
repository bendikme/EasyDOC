
function GanttDependency(item, parent) {

    var that = this;

    item.gantt = {
        type: 'dependency',
        item: this
    };

    var component;
    var cornerRadius = 7;

    var callbacks = {
        selectionChanged: []
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

    var arrow = {};

    this.draw = function () {

        var color = '#267cd3';

        if (item.From.IsMilestone) {
            color = '#161616';
        } else if (item.From.gantt.item.isCritical()) {
            color = '#dd2421';
        }

        var startToStart = item.Type == 'StartToStart' || item.Type == 0;
        var finishToStart = item.Type == 'FinishToStart' || item.Type == 1;
        var startToFinish = item.Type == 'StartToFinish' || item.Type == 2;
        var finishToFinish = item.Type == 'FinishToFinish' || item.Type == 3;

        var params = parent.getParams();

        var sourceX = Math.round(parent.getX(moment(startToStart || startToFinish ? item.From.StartDate : item.From.EndDate).unix()));
        var targetX = Math.round(parent.getX(moment(startToStart || finishToStart ? item.To.StartDate : item.To.EndDate).unix()));

        var sourceY = item.From.gantt.position * params.rowHeight + 8.5;
        var targetY = item.To.gantt.position * params.rowHeight + 8.5;

        var inset = 8;

        if (component != null) {
            component.remove();
        }

        component = new Kinetic.Group({
            x: sourceX,
            y: sourceY + 1
        });

        var offsetY = item.To.IsMilestone ? -8 : 0;

        var tX = targetX - sourceX + offsetY;
        var tY = targetY - sourceY;

        var aX = 0;
        var aY = 0;
        var ar = 0;

        var path = new PathBuilder();
        path.moveTo(0, 0);

        var direction = item.From.gantt.position < item.To.gantt.position ? 1 : -1;

        if (finishToStart) {
            if (targetX < sourceX || item.To.IsMilestone) {
                path.spathTo([[0, 0], [inset, 0], [inset, tY / 2], [tX - inset - 4, tY / 2], [tX - inset - 4, tY], [tX, tY]], cornerRadius);
                ar = -90;
            } else {
                var firstX = Math.max(tX, inset);
                path.spathTo([[0, 0], [firstX, 0], [firstX, tY - 13 * direction]], cornerRadius);
                aX = tX > inset ? 0 : inset;
                aY = -8 * direction;
                ar = direction == 1 ? 0 : 180;
            }
        } else if (startToFinish) {
            path.spathTo([[0, 0], [-inset, 0], [-inset, tY / 2], [tX + inset + 4, tY / 2], [tX + inset + 4, tY], [tX, tY]], cornerRadius);
            ar = 90;
        } else if (startToStart) {
            firstX = Math.min(tX - inset * 2, -inset * 2);
            path.spathTo([[0, 0], [firstX, 0], [firstX, tY], [tX, tY]], cornerRadius);
            ar = -90;
        } else if (finishToFinish) {
            firstX = Math.max(tX + inset * 2, inset * 2);
            path.spathTo([[0, 0], [firstX, 0], [firstX, tY], [tX, tY]], cornerRadius);
            ar = 90;
        }

        arrow.hit = new Kinetic.Path({
            data: path.getPath(),
            fill: 'transparent',
            stroke: 'transparent',
            strokeWidth: 20
        });

        arrow.path = new Kinetic.Path({
            data: path.getPath(),
            fill: color,
            stroke: color,
            strokeWidth: 1
        });

        arrow.arrow = new Kinetic.Path({
            x: tX + aX,
            y: tY + aY,
            rotation: ar,
            data: new PathBuilder()
			    .moveTo(0, 0)
			    .lineTo(-5, -5)
			    .hlineTo(5)
			    .close()
			    .getPath(),
            fill: color
        });

        component.add(arrow.hit);
        component.add(arrow.path);
        component.add(arrow.arrow);

        component.on('mouseover', function () {
            if (!item.isSelected) {
                arrow.path.strokeWidth(2);
                arrow.path.draw();
            }
        });

        component.on('mouseout', function () {
            if (!item.isSelected) {
                that.deselect();
            }
        });

        component.on('click', function () {
            item.isSelected = !item.isSelected;
            arrow.path.strokeWidth(item.isSelected ? 3 : 2);
            arrow.path.draw();
            parent.invalidate();
            fireEvent('selectionChanged', that);
        });

        return component;
    };

    this.delete = function () {
        component.remove();
    }

    this.deselect = function () {
        arrow.path.strokeWidth(1);
        item.isSelected = false;
        parent.invalidate();
    };

    this.getComponent = function () {
        return component;
    };

    function shadeColor(color, percent) {
        var offset = color[0] == '#' ? 1 : 0;
        var num = parseInt(color.slice(offset), 16), amt = Math.round(2.55 * percent), r = (num >> 16) + amt, g = (num >> 8 & 0x00FF) + amt, b = (num & 0x0000FF) + amt;
        return "#" + (0x1000000 + (r < 255 ? r < 1 ? 0 : r : 255) * 0x10000 + (g < 255 ? g < 1 ? 0 : g : 255) * 0x100 + (b < 255 ? b < 1 ? 0 : b : 255)).toString(16).slice(1);
    }
}