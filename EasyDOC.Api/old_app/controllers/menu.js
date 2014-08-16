(function () {
    'use strict';

    // Controller name is handy for logging
    var controllerId = 'MenuController';

    // Define the controller on the module.
    // Inject the dependencies. 
    // Point to the controller definition function.
    angular.module('app').controller(controllerId,
        ['$scope', '$q', 'datacontext', 'toolbar', 'permissions', 'common', menu]);

    function menu($scope, $q, datacontext, toolbar, permissions, common) {

        $scope.toolbar = toolbar;

        permissions.whenReady(function () {

            var menuItems = {
                items: [
                {
                    name: 'Project Managment',
                    items: [
                        {
                            name: 'Projects',
                            cssClass: 'project',
                            visible: permissions.hasPermission('Project', 'View')
                        }, {
                            name: 'Customers',
                            cssClass: 'customer',
                            visible: permissions.hasPermission('Customer', 'View')
                        }, {
                            name: 'Employees',
                            cssClass: 'employee',
                            visible: permissions.hasPermission('Employee', 'View')
                        }, {
                            name: 'Vendors',
                            cssClass: 'vendor',
                            visible: permissions.hasPermission('Vendor', 'View')
                        }
                    ]
                }, {
                    name: 'Documentation',
                    items: [
                        {
                            name: 'Documentation',
                            cssClass: 'documentation',
                            visible: permissions.hasPermission('Documentation', 'View')
                        }, {
                            name: 'Chapters',
                            cssClass: 'chapter',
                            visible: permissions.hasPermission('Chapter', 'View')
                        }, {
                            name: 'Maintenance',
                            cssClass: 'maintenance',
                            visible: permissions.hasPermission('Maintenance', 'View')
                        }, {
                            name: 'Safety',
                            cssClass: 'safety',
                            visible: permissions.hasPermission('Safety', 'View')
                        }, {
                            name: 'Files',
                            cssClass: 'folder',
                            visible: permissions.hasPermission('Folder', 'View'),
                            id: 1
                        }
                    ]
                }, {
                    name: 'Components',
                    items: [
                        {
                            name: 'Components',
                            cssClass: 'component',
                            visible: permissions.hasPermission('Component', 'View')
                        }, {
                            name: 'Series',
                            cssClass: 'componentseries',
                            visible: permissions.hasPermission('ComponentSeries', 'View')
                        }, {
                            name: 'Categories',
                            cssClass: 'category',
                            visible: permissions.hasPermission('Category', 'View'),
                            id: 1
                        }
                    ]
                }, {
                    name: 'Access',
                    items: [
                        {
                            name: 'Users',
                            cssClass: 'user',
                            visible: permissions.hasPermission('User', 'View')
                        }, {
                            name: 'Roles',
                            cssClass: 'role',
                            visible: permissions.hasPermission('Role', 'View')
                        }
                    ]
                }, {
                    name: 'Other',
                    items: [
                        {
                            name: 'Experiences',
                            cssClass: 'experience',
                            visible: permissions.hasPermission('Experience', 'View')
                        }
                    ]
                }, {
                    name: 'Recent items',
                    items: [

                    ]
                }
                ]
            };

            menuItems.items.forEach(function (mainItem) {
                mainItem.visible = mainItem.items.some(function (item) {
                    return item.visible;
                });
            });

            common.mostVisitedCallback = function (mostVisited) {
                var topTen = _.first(mostVisited, 10);
                var menuItem = _.findWhere(menuItems.items, { name: 'Recent items' });

                menuItem.items = [];
                topTen.forEach(function (item) {
                    menuItem.items.push({
                        name: item.entity.ProjectNumber ? (item.entity.ProjectNumber + ' - ' + item.entity.Name) : item.entity.Name,
                        cssClass: item.entity.entityAspect._entityKey.entityType.shortName.toLowerCase(),
                        visible: true,
                        id: item.entity.Id
                    });
                });

                menuItem.visible = topTen.length > 0;
            };

            $scope.menu = menuItems;
        });
    }
})();