(function () {

    'use strict';

    angular.module('app').directive('toolbar', function ($window) {

        return {
            restrict: 'E',
            templateUrl: '/app/templates/toolbar.html',
            replace: true,
            scope: true,
            controller: function ($scope, $timeout) {

                var timer;

                $scope.tooltip = '';
                $scope.isTooltipVisible = false;

                $scope.showTooltip = function (event, button) {
                    $scope.button = button;
                    $scope.tooltipX = event.clientX;
                    $scope.tooltipY = event.clientY + 20;

                    timer = $timeout(function () {
                        $scope.isTooltipVisible = true;
                    }, 750);
                }

                $scope.hideTooltip = function () {
                    $scope.isTooltipVisible = false;
                    $timeout.cancel(timer);
                }
            }
        };
    });
})();