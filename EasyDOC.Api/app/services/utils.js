angular.module('app').factory('utils', function () {

    return new function () {

        this.getFormattedSize = function (filesize) {

            var abbr = ['bytes', 'kB', 'MB', 'GB'];

            var i = 0;
            var size = filesize || 0;
            while (size > 1024) {
                size /= 1024;
                ++i;
            }

            return (i == 0 ? size + ' ' : size.toFixed(2)) + abbr[i];
        };

        this.generateUid = function (separator) {
            var delim = separator || '-';

            function s4() {
                return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
            }

            return (s4() + s4() + delim + s4() + delim + s4() + delim + s4() + delim + s4() + s4() + s4());
        };
    };
});