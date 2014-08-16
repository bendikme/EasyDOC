(function() {
    'use strict';

    // Controller name is handy for logging
    var controllerId = 'browser';

    // Define the controller on the module.
    // Inject the dependencies. 
    // Point to the controller definition function.
    angular.module('app').controller(controllerId,
        ['$scope', 'common', browser]);

    function browser($scope, common) {

    }
})();