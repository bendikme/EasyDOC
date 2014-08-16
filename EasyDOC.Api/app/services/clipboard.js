angular.module('app').factory('clipboard', function() {

    return new function() {

        this.copy = function(type, items, resource) {

            if (!$.isArray(items)) {
                items = [items];
            }

            var ids = _.pluck(items, 'Id');
            var clipboard = {
                type: type,
                resource: resource,
                ids: ids
            };

            sessionStorage.setItem('clipboard', JSON.stringify(clipboard));
        };

        this.paste = function() {
            var items = sessionStorage.getItem('clipboard');
            return JSON.parse(items);
        };
    };
});