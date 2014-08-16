(function () {
    'use strict';

    var controllerId = 'details';

    angular.module('app').controller(controllerId,
	    ['$scope', '$rootScope', '$state', '$location', 'common', 'datacontext', 'toolbar', 'settings', 'modal', 'permissions', details]);

    function details($scope, $rootScope, $state, $location, common, datacontext, toolbar, settings, modal, permissions) {

        var refreshButton = 1;
        $scope.tags = [];

        toolbar.items = [{
            click: deleteItem,
            disabled: false,
            visible: true,
            icon: 'fa-trash-o'
        }, {
            click: refreshList,
            icon: 'fa-refresh',
            visible: true,
            disabled: false
        }];


        //#region Internal Methods        

        var entitySettings = settings.detailViewSettings;

        function updateButtonStates() {
            if (toolbar.items.length) {
                toolbar.items[refreshButton].disabled = datacontext.hasNewItems();
            }
        }

        var type = $state.params.maintype;
        var params = entitySettings[type];

        permissions.whenReady(function () {
            params.tabs.forEach(function (tab) {
                if (!tab.type) {
                    tab.visible = true;
                } else {
                    tab.visible = permissions.hasPermission(tab.type, 'View');
                }

                setTimeout(function () {
                    $scope.tabs.forEach(function (tab) {
                        if (tab != selectedTab) {
                            tab.active = false;
                        }
                    });
                }, 0);
            });
        });

        $scope.$on('$stateChangeSuccess', function () {
            $scope.header = '/app/partials/' + type + '/header.html';
            $scope.tabs = params.tabs;

            selectedTab = _.findWhere(params.tabs, { type: $state.params.type }) || selectedTab || $scope.tabs[0];
            selectedTab.active = true;

        });

        $scope.$on('$stateChangeStart', function (fromState, fromParams, toState, toParams) {
            if (!(toState.name == 'details' || toState.name == 'details.list')) {
                selectedTab.active = false;
            }
        });

        var query = datacontext.createQuery()
            .from(params.from)
            .expand(params.expand)
            .where('Id', '==', parseInt($state.params.id))
            .inlineCount();

        datacontext.executeQuery(query, entityLoaded, errorLoading);

        $scope.tagsChanged = function (value) {
            $scope.item.Tags = $scope.tags.map(function (tag) {
                return tag.text;
            }).join();
        };

        function getTags() {
            return $scope.item.Tags.split(',').map(function (tag) {
                return { text: tag };
            });
        }

        function entityLoaded(data) {

            if (data.inlineCount > 0) {
                $scope.item = data.results[0];
                $scope.loaded = true;

                if ($scope.item.Tags) {

                    $scope.tags = getTags();

                    $scope.$watch('item.Tags', function () {
                        $scope.tags = getTags();
                    });
                }

                setTimeout(function () {
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

        function updateGrid() {
            $scope.$broadcast('load');
        }

        function deleteItem() {
            $scope.item.entityAspect.setDeleted();
        }

        function refreshList() {
            $scope.$broadcast('refreshList');
        }

        setTimeout(function () {
            updateGrid();
        }, 0);

        //#endregion

        $rootScope.$watch(function () {
            updateButtonStates();
        });

        $scope.getImagePreview = function (file, width, height) {
            return $scope.getUrl(file, true) + '?width=' + width + '&height=' + height + '&scale=upscalecanvas&bgcolor=white';
        };

        $scope.getUrl = function (file, returnUrl) {
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
        };

        $scope.loaded = false;
        $scope.item = null;

        $scope.getFormattedSize = common.getFormattedSize;

        var selectedTab;
        $scope.selectTab = function (tab, test) {
            selectedTab = tab;

            if (!tab.template) {
                $state.go('details.list', { type: tab.type });
            } else {
                $state.go('details');
            }

            tab.active = true;
        };

        $scope.select = function (route, property) {
            if (window.opener == null) {

                var routeName = route;
                var actualRoute = route;

                if (route == 'folder' || route == 'category') {
                    routeName += '/1';
                    if (route == 'folder') {
                        actualRoute = 'file';
                    }
                }

                modal.openSelectListWindow(routeName, false, '', function (selection) {
                    console.log(selection);
                    datacontext.fetchEntitiesByKey(settings.getResource(actualRoute), selection.ids, function (data) {
                        $scope.item[property] = data.results[0];
                    });
                });
            } else {
                toastr.warning('Can\'t open a second popup window.', 'Warning!');
            }
        };

        $scope.hasNewItems = function () {
            return datacontext.hasNewItems();
        };
    }
})();