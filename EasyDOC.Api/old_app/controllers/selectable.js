
function Selectable(model, property) {

    this.getSelectedItems = function () {
        return _.where(model[property], { isSelected: true });
    };


    this.selectAll = function () {
        _.each(model[property], (function (item) {
            item.isSelected = true;
        }));
    };


    this.deselectAll = function () {
        _.each(model[property], (function (item) {
            item.isSelected = false;
        }));
    };

    this.selectNext = function () {

        var last = _.last(this.getSelectedItems());

        if (last) {
            var nextIndex = Math.min(_.indexOf(model[property], last) + 1, model[property].length - 1);
            this.deselectAll();
            model[property][nextIndex].isSelected = true;
        }
    };

    this.selectPrevious = function () {

        var first = _.first(this.getSelectedItems());

        if (first) {
            var previousIndex = Math.max(_.indexOf(model[property], first) - 1, 0);
            this.deselectAll();
            model[property][previousIndex].isSelected = true;
        }
    };

    this.selectItem = function (item, event, cursor) {

        var index = _.indexOf(model[property], item);

        if (!event.isDefaultPrevented()) {
            if (event.shiftKey) {

                var start = cursor;
                var end = index;

                if (end < start) {
                    end = [start, start = end][0];
                }

                model[property].slice(start, end + 1).forEach(function (i) {
                    i.isSelected = true;
                });

            } else if (event.ctrlKey) {
                item.isSelected = !item.isSelected;
            } else if (event.target.localName !== 'input') {
                var count = this.getSelectedItems().length;
                var status = item.isSelected;
                this.deselectAll();
                item.isSelected = count == 1 ? !status : true;
            }
        }

        return index;
    };
}