
angular.module('app').factory('keyboard', function (settings) {

    var handler;

    $(window).on('keydown', function (event) {
        if (_.isFunction(handler)) {
            handler(event);
        }
    });

    return new function () {

        this.setHandler = function (func) {
            handler = func;
        };

    };
});
