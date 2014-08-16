(function() {

    angular.module('app').controller('list', function($scope, $state, settings) {

        var type = $state.params.linkType || $state.params.type || 'user';
        var entityInfo = _.findWhere(settings.queryParams, { route: type });

        if (entityInfo.indirect) {
            _.extend(entityInfo, entityInfo.indirect[$state.params.maintype]);
        }
    });
})();