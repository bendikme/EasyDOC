(function () {

    'use strict';

    angular.module('app').directive('datagrid', function () {

        return {
            restrict: 'E',
            templateUrl: '/app/datagrid/datagrid.html',
            transclude: true,
            replace: true,
            controller: 'datagrid',
            scope: {
                ready: '=',
                config: '@',
                items: '='
            },
            link: function (scope, element, attrs) {

                var table = $('#table', element);

                scope.$watch('cursor', function (value) {

                    var elm = $('#table-body > tbody > tr', element)[value];

                    if (elm) {
                        var elmTop = $(elm).offset().top;
                        var elmHeight = $(elm).height();
                        var tableTop = table.offset().top;
                        var tableHeight = table.height();
                        var scroll = table.scrollTop();

                        var diff = elmTop + elmHeight - tableTop + scroll;

                        if (diff > tableHeight + scroll) {
                            table.scrollTop(diff - tableHeight);
                        } else if (diff - elmHeight < scroll) {
                            table.scrollTop(diff - elmHeight);
                        }
                    }
                });


                var resizeGrid = function () {
                    table.height($(window).height() - table.offset().top - 29);
                }

                $(window).on('resize', resizeGrid);

                scope.$on('$destroy', function () {
                    $(window).off('resize', resizeGrid);
                });
            }
        };
    });
})();