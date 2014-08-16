angular.module('app').factory('modal', function() {

    return new function() {

        var messagePostedCallback;
        var width = 1200;
        var height = 800;

        window.addEventListener('message', function(event) {
            $.isFunction(messagePostedCallback) && messagePostedCallback(event.data);
        });

        this.setModalSize = function(w, h) {
            width = w;
            height = h;
        };


        this.openImportWindow = function(fnCallback) {
            messagePostedCallback = fnCallback;
            window.open(
                '//' + window.location.host + '/' + 'import',
                'select',
                'location=no,width=' + width + ',height=' + height);
        };

        this.openSelectListWindow = function(type, many, filter, fnCallback) {

            messagePostedCallback = fnCallback;
            window.open(
                '//' + window.location.host + '/' + type + '?mode=' + (many ? 'many' : 'one') + '&filter=' + filter,
                'select',
                'location=no,width=' + width + ',height=' + height);
        };

        this.postMessageAndCloseWindow = function(message) {
            console.log(window.location.host),
            window.opener.postMessage(message, 'http://' + window.location.host);
            window.close();
        };
    };
});