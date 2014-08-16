angular.module('app').directive('dynamic', function($compile) {
    return {
        restrict: 'EA',
        replace: true,
        link: function(scope, element, attrs) {
            scope.$watch(attrs.dynamic, function(html) {
                element.html(html);
                $compile(element.contents())(scope);
            });
        }
    };
});