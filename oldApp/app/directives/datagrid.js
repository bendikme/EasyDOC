(function() {

    'use strict';

    angular.module('app').directive('datagrid', function() {
        return {
            restrict: 'E',
            templateUrl: '/app/templates/datagrid.html',
            transclude: true,
            replace: true,
            controller: 'datagrid',
            scope: {
                ready: "=",
            }
        };
    }).directive('column', function() {
        return {
            restrict: 'E',
            replace: true,
            require: '^datagrid',
            link: function(scope, element, attrs, controller) {
                controller.addColumn(attrs, element.html());
            }
        };
    });
})();