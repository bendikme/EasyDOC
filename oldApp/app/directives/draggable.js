(function () {
    angular.module('app').directive('draggable', [function () {

        return {
            restrict: 'A',
            link: function (scope, element) {

                var handleDragStart = function (e) {

                    if (!e.originalEvent.firstItem) {

                        scope.onDragStart(scope.item);
                        e.originalEvent.dataTransfer.effectAllowed = 'move';
                        e.originalEvent.firstItem = true;

                        if (scope.getSelectedItems) {
                            e.originalEvent.dataTransfer.setData('text/plain',
                                angular.toJson({
                                    data: _.map(scope.getSelectedItems(), function (item) {
                                        return { id: item.Id, type: item.entityAspect._entityKey.entityType.shortName };
                                    })
                                }));
                        } else {
                            e.originalEvent.dataTransfer.setData('text/plain',
                                angular.toJson({ data: [{ id: scope.item.Id, type: scope.item.entityAspect._entityKey.entityType.shortName }] }));
                        }
                    }
                };

                var handleDragEnd = function (e) {
                    e.preventDefault();
                    scope.onDragEnd();
                };

                element.attr("draggable", "true");
                element.bind("dragstart", handleDragStart);
                element.bind("dragend", handleDragEnd);
            }
        };
    }]);

    angular.module('app').directive('droppable', [function () {

        return {
            restrict: 'A',
            link: function (scope, element) {
                var elm = element;

                var dragAndDrop = {
                    handleDropleave: function (e) {
                        elm.removeClass("over");
                        scope.onDragLeave(scope.item, e);
                    },

                    handleDragEnter: function (e) {
                        if (e.preventDefault) e.preventDefault();
                        elm.addClass("over");
                        scope.onDragEnter(scope.item, e);
                    },

                    handleDragOver: function (e) {
                        if (e.preventDefault) e.preventDefault();
                        elm.addClass("over");
                        scope.onDragOver(scope.item, e);
                        return false;
                    },

                    handleDropped: function (e) {
                        if (e.stopPropagation) e.stopPropagation(); // stops the browser from redirecting..

                        var data = e.originalEvent.dataTransfer.getData('text/plain');
                        if (data) {
                            scope.onDrop(scope.item, angular.fromJson(data));
                        } else if (e.originalEvent.dataTransfer.files.length) {
                            scope.onDropFiles(scope.item, e.originalEvent.dataTransfer.files);
                        }

                        elm.removeClass("over"); // for removing highlighting effect on droppable object
                        return false;
                    }
                };

                element.bind("dragenter", dragAndDrop.handleDragEnter);
                element.bind("dragover", dragAndDrop.handleDragOver);
                element.bind("dragleave", dragAndDrop.handleDropleave);
                element.bind("drop", dragAndDrop.handleDropped);
            }
        };
    }]);

}());