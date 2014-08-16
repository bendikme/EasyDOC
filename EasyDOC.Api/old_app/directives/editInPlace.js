angular.module('app').directive('easyEdit', function (permissions) {

    return {
        restrict: 'E',
        replace: false,
        templateUrl: '/app/partials/editInPlace.html',
        scope: {
            model: '=',
            property: '@',
            show: '=',
            editMode: '=active'
        },
        link: function (scope, element, attrs) {

            var unregisterWatch = scope.$watch('model', function (value) {
                if (scope.model != undefined) {
                    permissions.whenReady(function () {
                        scope.editable = permissions.hasPermission(scope.model.entityAspect._entityKey.entityType.shortName, 'Modify', scope.model.CreatorId);
                    });
                    unregisterWatch();
                }
            });

            scope.startEdit = function (event) {

                scope.view = {
                    value: scope.model[scope.property]
                };

                scope.editMode = true;
                event.preventDefault();
            };

            scope.cancelChanges = function (event) {
                scope.editMode = false;
                if (event) event.preventDefault();
            };

            scope.saveChanges = function (event) {
                scope.model[scope.property] = scope.view.value;
                scope.cancelChanges(event);
            };
        }
    };
});

angular.module('app').directive('easySelect', function (datacontext, permissions) {

    var template = '<button class="editable-link pull-right" ng-if="editable && !editMode" ng-click="startEdit($event)"><i class="fa fa-list-ol"></i></button>' +
            '<button class="editable-link pull-right" ng-if="editMode" ng-mousedown="saveChanges($event)"><i class="fa fa-save"></i></button>' +
            '<div ng-if="!editMode" style="text-overflow: ellipsis; overflow: hidden; width: 70%; display: inline-block;">{{show}}</div>' +
            '<select ng-click="preventDefault($event)" ng-model="view.value" ng-options="r.Name as r.Name for r in options | orderBy:\'Name\'" ng-enter="saveChanges()" ng-esc="cancelChanges()" ng-blur="cancelChanges()" ng-if="editMode" type="text" class="editMode" ng-model="view.value" autofocus></select>';
    return {
        restrict: 'E',
        replace: false,
        template: template,
        scope: {
            model: '=',
            property: '@',
            enumtype: '@',
            show: '='
        },
        link: function (scope, element, attrs) {

            var type = "";

            var unregisterWatch = scope.$watch('model', function (value) {
                if (scope.model != undefined) {

                    type = scope.model.entityAspect._entityKey.entityType.shortName;

                    permissions.whenReady(function () {
                        scope.editable = permissions.hasPermission(type, 'Modify', scope.model.CreatorId);
                    });
                    unregisterWatch();
                }
            });

            scope.preventDefault = function (event) {
                event.preventDefault();
            };

            scope.startEdit = function (event) {

                var query = datacontext.createQuery().from(scope.enumtype);
                datacontext.executeQuery(query)
                    .then(function (data) {
                        var array = [];
                        for (var i = 0; i < data.results.length; ++i) {
                            array.push({ Id: parseInt(i), Name: data.results[i] });
                        }
                        scope.options = array;
                    });

                scope.view = {
                    value: scope.model[scope.property]
                };

                scope.editMode = true;
                event.preventDefault();
            };

            scope.cancelChanges = function (event) {
                scope.editMode = false;
                if (event) event.preventDefault();
            };

            scope.saveChanges = function (event) {
                scope.model[scope.property] = scope.view.value;
                scope.cancelChanges(event);
            };
        }
    };
});