
function Selectable(activeScope) {

    var scope = activeScope;

    this.getSelectedItems = function () {
        return _.where(scope.items, { isSelected: true });
    };


    this.selectAll = function () {
        _.each(scope.items, function (item) {
            item.isSelected = true;
        });
    };


    this.deselectAll = function () {
        _.each(scope.items, function (item) {
            item.isSelected = false;
        });
    };


    this.selectItem = function(item, event, index, cursor) {
        if (event.target.localName == 'td') {
            if (event.shiftKey) {

                var start = cursor;
                var end = index;

                if (end < start) {
                    end = [start, start = end][0];
                }

                _.each(scope.items.slice(start, end + 1), function(i) {
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

            return index;
        }

        return null;
    };

}