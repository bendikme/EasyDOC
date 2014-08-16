(function () {
    angular.module('app').directive('chapterEditor', ['$modal', function ($modal) {

        return {
            restrict: 'A',
            link: function (scope, element) {

                element.click(function () {
                    $modal.open({
                        templateUrl: 'chapterEditor.html',
                        controller: modalInstanceCtrl,
                        resolve: {
                            item: function () {
                                return scope.item;
                            }
                        }
                    });
                });
            }
        };
    }]);

    var modalInstanceCtrl = function ($scope, $modalInstance, item) {

        $scope.item = item;

        $scope.ok = function () {
            $modalInstance.close($scope.item);
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('cancel');
        };
    };
}());