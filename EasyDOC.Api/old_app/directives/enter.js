angular.module('app').directive('ngEnter', function() {
    return function(scope, element, attrs) {
        element.bind("keydown keypress", function(event) {
            if (event.which === 13) {
                scope.$apply(function() {
                    scope.$eval(attrs.ngEnter);
                });

                event.preventDefault();
            }
        });
    };
});

angular.module('app').directive('ngEsc', function() {
    return function(scope, element, attrs) {
        element.bind("keydown keypress", function(event) {
            if (event.which === 27) {
                scope.$apply(function() {
                    scope.$eval(attrs.ngEsc);
                });

                event.preventDefault();
            }
        });
    };
});