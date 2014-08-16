angular.module('app').factory('signalr', function ($rootScope) {

    var hub = $.connection.entityHub;

    hub.client.entitiesChanged = function (result) {
        toastr.info("Some entities have been updated.", "Info!");
    };

    $.connection.hub.start();

    return {
        
    };
});