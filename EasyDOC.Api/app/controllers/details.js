(function() {
    'use strict';

    var controllerId = 'details';

    angular.module('app').controller(controllerId,
    ['$scope', '$rootScope', '$state', '$location', 'common', 'datacontext', 'settings', 'modal', 'permissions', details]);

    function details($scope, $rootScope, $state, $location, common, datacontext, settings, modal, permissions) {

        $scope.tags = [];

        var entitySettings = settings.detailViewSettings;

        var type = $state.params.maintype;
        var params = entitySettings[type];

        $scope.async = {};
        $scope.loading = {};

        if (!params) return;

        _.each(params.tabs, function(tab) {
            if (tab.sections) {
                _.each(tab.sections, function(section) {
                    _.each(section.fields, function(field) {
                        if (field.type == 'async') {
                            var id = common.generateUid();
                            field.model = id;
                            $scope.async[id] = '';

                            $scope.$watch('item.' + field.property, function(value) {
                                $scope.async[id] = value ? value.Name : null;
                            });
                        }
                    });
                });
            }
        });

        $scope.header = '/app/partials/' + type + '/header.html';
        $scope.tabs = params.tabs;

        $scope.saveChanges = function() {
            datacontext.saveChanges();
        };

        $scope.hasChanges = function() {
            return datacontext.hasChanges();
        };

        $scope.cancelChanges = function() {
            datacontext.cancelChanges();
        };

        $scope.getAsync = function(from, search) {

            if (_.isString(search) && search.length > 0) {

                $scope.loading[from] = true;

                var query = datacontext.createQuery()
                    .from(from)
                    .where('Name', 'Contains', search)
                    .orderBy('Name')
                    .take(10);

                return datacontext.executeQuery(query, true).then(function(data) {
                    $scope.loading[from] = false;
                    return data.results;
                });
            }

            return [];
        };

        var destroyed = false;

        $scope.$on('$destroy', function() {
            destroyed = true;
        });

        $scope.$on('$stateChangeSuccess', function(toState, toParams, fromState, fromParams) {
            if (toParams.name != 'details') {
                selectedTab && (selectedTab.active = false);
                selectedTab = _.findWhere(params.tabs, { type: $state.params.type }) || selectedTab || $scope.tabs[0];
                selectedTab.active = true;
            }
        });

        var query = datacontext.createQuery()
            .from(params.from)
            .expand(params.expand)
            .where('Id', '==', parseInt($state.params.id))
            .inlineCount();

        datacontext.executeQuery(query)
            .then(entityLoaded)
            .catch(errorLoading);

        $scope.tagsChanged = function(value) {
            $scope.item.Tags = $scope.tags.map(function(tag) {
                return tag.text;
            }).join();
        };

        function getTags() {
            return $scope.item.Tags.split(',').map(function(tag) {
                return { text: tag };
            });
        }

        function entityLoaded(data) {

            if (data.inlineCount > 0) {
                $scope.item = data.results[0];
                $scope.loaded = true;

                if ($scope.item.Tags) {

                    $scope.tags = getTags();

                    $scope.$watch('item.Tags', function() {
                        $scope.tags = getTags();
                    });
                }

                setTimeout(function() {
                    $scope.$broadcast('itemLoaded', [$scope.item]);
                }, 1000);
                common.addToMostVisited($scope.item);
            } else {
                toastr.warning('The requested record was not found.', 'Warning!');
            }
        }

        function errorLoading(error) {
            toastr.error('Error fetching records from server.', 'Error!');
            console.error(error);
        }

        function deleteItem() {
            $scope.item.entityAspect.setDeleted();
        }

        $scope.getImagePreview = function(file, width, height) {
            return $scope.getUrl(file, true) + '?width=' + width + '&height=' + height + '&scale=upscalecanvas&bgcolor=white';
        };

        $scope.getUrl = function(file, returnUrl) {
            if (file != null) {
                var url = '/' + file.Name + '.' + file.Type;
                var parent = file.Folder;

                while (parent != null) {
                    url = '/' + parent.Name + url;
                    parent = parent.Parent;
                }

                if (returnUrl) {
                    return url;
                } else {
                    window.open(url, '_blank');
                }
            }

            return null;
        };

        $scope.loaded = false;
        $scope.item = null;

        $scope.getFormattedSize = common.getFormattedSize;

        var selectedTab = {};
        $scope.selectTab = function(tab) {

            if (destroyed) return;

            selectedTab.active = false;
            selectedTab = tab;

            if (!tab.template && !tab.sections) {
                $state.go('details.list', { type: tab.type });
            } else {
                $state.go('details');
            }

            tab.active = true;
        };

        $scope.hasNewItems = function() {
            return datacontext.hasNewItems();
        };
    }
})();