(function () {

    'use strict';

    angular.module('app').directive('ganttChart', function ($timeout, $window) {

        return {
            restrict: 'E',
            templateUrl: '/app/templates/gantt.html',
            transclude: true,
            replace: true,
            controller: 'tasks',
            scope: {
                items: '='
            },
            link: function (scope, element, attrs) {

                var gantt = $('#gantt-chart', element);
                var ganttHeader = $('#gantt-header', element);

                function onWindowResized() {
                    var height = $(window).height() - $('#grid', element).offset().top - $('#statusbar').height() - 56;
                    gantt.height(height);
                    scope.ganttChartHeight = $('#table-body', element).height();
                }

                var resizeDebounced = _.debounce(onWindowResized, 200);
                $(window).on('resize', resizeDebounced);

                $timeout(function () {
                    onWindowResized();

                    var table = $('#table', element);
                    var tableHeader = $('#table-header', element);

                    $('#table-body').resize(function () {
                        scope.ganttChartHeight = $('#table-body', element).height();
                    });

                    $('#table-body-wrapper').width(600);
                    $('#table-header-wrapper').width(600);

                    table.scroll(function (e) {
                        gantt.scrollTop(e.target.scrollTop);
                        tableHeader.scrollLeft(e.target.scrollLeft);
                    });

                    gantt.scroll(function (e) {
                        table.scrollTop(e.target.scrollTop);
                        ganttHeader.scrollLeft(e.target.scrollLeft);
                    });

                }, 2000);

                scope.$on('$destroy', function () {
                    var table = $('#table', element);

                    table.off();
                    gantt.off();

                    $(window).off('resize', resizeDebounced);
                });
            }
        };
    });
})();