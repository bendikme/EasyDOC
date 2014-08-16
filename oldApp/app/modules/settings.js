(function () {
    'use strict';

    // Factory name is handy for logging
    var serviceId = 'settings';

    angular.module('app').factory(serviceId, ['$http', settings]);

    var generateUid = function (separator) {
        var delim = separator || "-";

        function S4() {
            return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
        }

        return (S4() + S4() + delim + S4() + delim + S4() + delim + S4() + delim + S4() + S4() + S4());
    };

    var detailViewSettings = {
        project: {
            from: 'Projects',
            expand: 'Customer,Creator,Editor,ProjectManager',
            tabs: [
                { title: 'Properties', template: '/app/partials/project/properties.html' },
                { title: 'Attachments', type: 'projectfile' },
                { title: 'Order confirmations', type: 'orderconfirmation' },
                { title: 'Components', type: 'projectcomponent' },
                { title: 'Documentation', type: 'documentation' },
                { title: 'Maintenance', type: 'projectmaintenance' },
                { title: 'Safety', type: 'projectsafety' },
                { title: 'Chapters', type: 'chapter' },
                { title: 'Experiences', type: 'experience' }
            ],
        },
        orderconfirmation: {
            from: 'OrderConfirmations',
            expand: 'Creator,Editor,Project,Vendor,CustomerReference',
            tabs: [
                { title: 'Properties', template: '/app/partials/orderconfirmation/properties.html' },
                { title: 'Items', type: 'orderconfirmationitem' }
            ],
        },
        experience: {
            from: 'Experiences',
            expand: 'Creator,Editor,Project,Component',
            tabs: [
                { title: 'Properties', template: '/app/partials/experience/properties.html' }
            ],
        },
        file: {
            from: 'Files',
            expand: 'Creator,Editor',
            tabs: [
                { title: 'Projects', type: 'projectfile' },
                { title: 'Components', type: 'projectcomponent' },
                { title: 'Documentation', type: 'documentation' },
                { title: 'Maintenance', type: 'maintenance' },
                { title: 'Safety', type: 'safety' },
                { title: 'Chapters', type: 'chapterfile' }
            ],
        },
        employee: {
            from: 'Employees',
            expand: 'Account,ManagedProjects,Tasks',
            tabs: [
                { title: 'Properties', template: '/app/partials/employee/properties.html' },
                { title: 'Managed Projects', type: 'project' },
                { title: 'Tasks', type: 'employeetasks' }
            ],
        },
        category: {
            from: 'Categories',
            expand: 'Creator,Editor',
            tabs: [
                { title: 'Components', type: 'component' }
            ]
        },
        chapter: {
            from: 'Chapters',
            expand: 'Project,Creator,Editor',
            tabs: [
                { title: 'Content', template: '/app/partials/chapter/content.html' },
                { title: 'Properties', template: '/app/partials/chapter/properties.html' },
                { title: 'Documentation', type: 'documentationchapter' }
            ],
        },
        customer: {
            from: 'Customers',
            expand: 'Creator,Editor',
            tabs: [{ title: 'Projects', type: 'project', }],
        },
        component: {
            from: 'Components',
            expand: 'Vendor,Category,Series,Creator,Editor,Image,Image.Folder,Image.Folder.Parent,Image.Folder.Parent.Parent,Image.Folder.Parent.Parent.Parent',
            tabs: [
                { title: 'Properties', template: '/app/partials/component/properties.html' },
                { title: 'Attachments', type: 'componentfile' },
                { title: 'Maintenance', type: 'componentmaintenance' },
                { title: 'Safety', type: 'componentsafety' },
                { title: 'Project', type: 'projectcomponent' },
                { title: 'Experiences', type: 'experience' }
            ]
        },
        documentation: {
            from: 'Documentations',
            expand: 'Project,Creator,Editor,Image,Image.Folder,Image.Folder.Parent,Image.Folder.Parent.Parent,Image.Folder.Parent.Parent.Parent',
            tabs: [
                { title: 'Properties', template: '/app/partials/documentation/properties.html' },
                { title: 'Chapters', type: 'documentationchapter' }
            ],
        },
        maintenance: {
            from: 'Maintenances',
            expand: 'Vendor,Creator,Editor,Manual,Image.Folder.Parent.Parent,Image.Folder.Parent.Parent.Parent',
            tabs: [
                { title: 'Properties', template: '/app/partials/maintenance/properties.html' },
                { title: 'Components', type: 'componentmaintenance' },
                { title: 'Project', type: 'projectmaintenance' }
            ],
        },
        role: {
            from: 'Roles',
            expand: 'Creator,Editor',
            tabs: [
                { title: 'Permissions', type: 'rolepermission' },
                { title: 'Users', type: 'userrole' }
            ]
        },
        safety: {
            from: 'Safeties',
            expand: 'Creator,Editor,Image,Image.Folder,Image.Folder.Parent,Image.Folder.Parent.Parent,Image.Folder.Parent.Parent.Parent',
            tabs: [
                { title: 'Properties', template: '/app/partials/safety/properties.html' },
                { title: 'Components', type: 'componentsafety' },
                { title: 'Projects', type: 'projectsafety' }
            ]
        },
        series: {
            from: 'ComponentSeries',
            expand: 'Vendor,Category,Creator,Editor',
            tabs: [
                { title: 'Properties', template: '/app/partials/series/properties.html' },
                { title: 'Attachments', type: 'componentfile' },
                { title: 'Maintenance', type: 'componentmaintenance' },
                { title: 'Safety', type: 'componentsafety' },
                { title: 'Project', type: 'projectcomponent' }
            ]
        },
        user: {
            from: 'Users',
            tabs: [
                { title: 'Properties', template: '/app/partials/user/properties.html' },
                { title: 'Roles', type: 'userrole' }
            ]
        },
        vendor: {
            from: 'Vendors',
            expand: 'Creator,Editor',
            tabs: [
                { title: 'Properties', template: '/app/partials/vendor/properties.html', },
                { title: 'Components', type: 'component' }
            ]
        },
    };

    var defaultEntityValues = {
        Category: { Name: 'New category' },
        Chapter: { Name: 'New chapter' },
        Component: { Name: 'New component' },
        ComponentFile: { Name: 'New attachment', Role: 'Other', IncludeInManual: true },
        ComponentSeries: { Name: 'New component series' },
        ComponentMaintenance: { IncludeInManual: true },
        ComponentSafety: { IncludeInManual: true, Role: 'SafetyRisk' },
        Customer: { Name: 'New customer' },
        Documentation: { Name: 'New documentation' },
        DocumentationChapter: { UniqueKey: generateUid, ChapterNumber: '1' },
        Employee: { Name: 'New employee' },
        Experience: { Name: 'New experience' },
        Folder: { Name: 'New folder' },
        Maintenance: { Name: 'New maintenance' },
        Permission: { Name: 'New permission' },
        Project: { ProjectNumber: 'P' + (new Date().getFullYear()) + '-???', Name: 'New project', Status: 'Planned' },
        ProjectFile: { Name: 'New attachment', Role: 'Other', IncludeInManual: true },
        ProjectComponent: { Count: 1, IncludeInManual: true, Unit: 'Pieces' },
        ProjectMaintenance: { IncludeInManual: true },
        ProjectSafety: { IncludeInManual: true, Role: 'SafetyRisk' },
        Role: { Name: 'New role' },
        RolePermission: { Read: 'All', Create: 'All', Update: 'All', Delete: 'All' },
        Safety: { Name: 'New safety' },
        Vendor: { Name: 'New vendor' },
        User: { Name: 'New user' }
    };

    var getDefaultValuesForEntity = function (entityType) {

        var entity = {};
        var values = defaultEntityValues[entityType];
        for (var property in values) {
            if (values.hasOwnProperty(property)) {
                var value = values[property];
                entity[property] = $.isFunction(value) ? value() : value;
            }
        }

        return entity;
    };

    var queryParameters = [{
        route: 'project',
        entityType: 'Project',
        resource: 'Projects',
        expand: 'Customer,Creator,Editor',
        sort: 'ProjectNumber',
        load: true,
        indirect: {
            customer: {
                fromId: 'CustomerId',
                toId: 'Id'
            },
            employee: {
                fromId: 'ProjectManagerId',
                toId: 'Id'
            }
        }
    }, {
        route: 'orderconfirmation',
        entityType: 'OrderConfirmation',
        resource: 'OrderConfirmations',
        expand: 'Vendor,Creator,Editor,Project',
        load: true,
        indirect: {
            project: {
                fromId: 'ProjectId',
                toId: 'Id'
            }
        }
    }, {
        route: 'orderconfirmationitem',
        entityType: 'OrderConfirmationItem',
        resource: 'OrderConfirmationItems',
        expand: 'Component',
        load: true,
        indirect: {
            orderconfirmation: {
                template: '/app/partials/orderconfirmation/items.html',
                fromId: 'OrderConfirmationId',
                toId: 'Id'
            }
        }
    }, {
        route: 'employee',
        entityType: 'Employee',
        resource: 'Employees',
        expand: 'Account,Creator,Editor',
        load: true
    }, {
        route: 'category',
        entityType: 'Category',
        resource: 'Categories',
        load: true,
        hierarchy: true,
        childRoute: 'component',
        childType: 'Component',
        childResource: 'Components',
        childParent: 'Category',
        expand: 'SubCategories, ParentCategory, SubCategories.SubCategories',
        childrenProperty: 'SubCategories',
        childParentPropertyId: 'CategoryId',
        parentProperty: 'ParentCategory',
        parentPropertyId: 'ParentCategoryId',
        hasChildren: function (item) {
            return item.SubCategories.length;
        }
    }, {
        route: 'role',
        entityType: 'Role',
        resource: 'Roles',
        load: true
    }, {
        route: 'experience',
        entityType: 'Experience',
        resource: 'Experiences',
        load: true,
        indirect: {
            project: {
                fromId: 'ProjectId',
                toId: 'Id'
            },
            component: {
                fromId: 'ComponentId',
                toId: 'Id'
            }
        }
    }, {
        route: 'permission',
        entityType: 'Permission',
        resource: 'Permissions',
        load: true
    }, {
        route: 'user',
        entityType: 'User',
        resource: 'Users',
        load: true
    }, {
        route: 'component',
        entityType: 'Component',
        resource: 'Components',
        expand: 'Category,Vendor,Creator,Editor',
        load: true,
        hierarchy: true,
        parentId: 'CategoryId',
        indirect: {
            vendor: {
                fromId: 'VendorId',
                toId: 'Id'
            },
            category: {
                fromId: 'CategoryId',
                toId: 'Id'
            }
        }
    }, {
        route: 'customer',
        entityType: 'Customer',
        resource: 'Customers',
        expand: 'Creator,Editor',
        load: true
    }, {
        route: 'file',
        entityType: 'File',
        resource: 'Files',
        expand: 'Creator,Editor',
        load: true,
        hierarchy: true,
        parentId: 'CatalogId',
    }, {
        route: 'folder',
        entityType: 'Folder',
        resource: 'Folders',
        load: true,
        hierarchy: true,
        childRoute: 'file',
        childType: 'File',
        childResource: 'Files',
        childParent: 'Folder',
        expand: 'Catalogs, Files, Parent, Catalogs.Catalogs',
        childrenProperty: 'Catalogs',
        childParentPropertyId: 'CatalogId',
        parentProperty: 'Parent',
        parentPropertyId: 'ParentId',
        hasChildren: function (item) {
            return item.Catalogs.length || item.Files.length;
        }
    }, {
        route: 'chapter',
        entityType: 'Chapter',
        resource: 'Chapters',
        expand: 'Project,Creator,Editor',
        load: true,
        indirect: {
            project: {
                fromId: 'ProjectId',
                toId: 'Id'
            }
        }
    }, {
        route: 'abstractchapter',
        entityType: 'AbstractChapter',
        resource: 'AllChapters',
        expand: 'Project,Creator,Editor',
        load: true
    }, {
        route: 'abstractcomponent',
        entityType: 'AbstractComponent',
        resource: 'AllComponents',
        expand: 'Vendor,Category,Creator,Editor',
        load: true,
    }, {
        route: 'documentation',
        entityType: 'Documentation',
        resource: 'Documentations',
        expand: 'Project,Creator,Editor',
        load: true,
        indirect: {
            project: {
                fromId: 'ProjectId',
                toId: 'Id'
            },
            file: {
                fromId: 'ImageId',
                toId: 'Id'
            }
        },
        copy: function (originalEntity) {
            return {
                ProjectId: originalEntity.ProjectId,
                Name: "Copy of " + originalEntity.Name
            };
        }
    }, {
        route: 'maintenance',
        entityType: 'Maintenance',
        resource: 'Maintenances',
        expand: 'Vendor,Manual,Creator,Editor',
        load: true,
        indirect: {
            file: {
                fromId: 'ManualId',
                toId: 'Id'
            }
        }
    }, {
        route: 'safety',
        entityType: 'Safety',
        resource: 'Safeties',
        expand: 'Creator,Editor',
        load: true,
        indirect: {
            file: {
                fromId: 'ImageId',
                toId: 'Id'
            }
        }
    }, {
        route: 'series',
        entityType: 'ComponentSeries',
        resource: 'ComponentSeries',
        expand: 'Vendor,Creator,Editor',
        load: true
    }, {
        route: 'vendor',
        entityType: 'Vendor',
        resource: 'Vendors',
        expand: 'Creator,Editor',
        load: true
    }, {
        multiKey: true,
        route: 'userrole',
        entityType: 'UserRole',
        resource: 'UserRoles',
        indirect: {
            role: {
                from: 'role',
                fromId: 'RoleId',
                fromType: 'Role',
                to: 'user',
                toId: 'userId',
                toProperty: 'User',
                toResource: 'Users',
                toExpand: null,
                template: '/app/partials/role/users.html',
                expand: 'User',
                sort: 'User.Name'
            },
            user: {
                from: 'user',
                fromId: 'UserId',
                fromType: 'User',
                to: 'role',
                toId: 'roleId',
                toProperty: 'Role',
                toResource: 'Roles',
                toExpand: null,
                template: '/app/partials/user/roles.html',
                expand: 'Role',
                sort: 'Role.Name'
            }
        }
    }, {
        multiKey: true,
        route: 'rolepermission',
        entityType: 'RolePermission',
        resource: 'RolePermissions',
        indirect: {
            role: {
                from: 'role',
                fromId: 'RoleId',
                fromType: 'Role',
                to: 'permission',
                toId: 'PermissionId',
                toProperty: 'Permission',
                toResource: 'Permissions',
                toExpand: null,
                template: '/app/partials/role/permissions.html',
                expand: 'Permission',
                sort: 'Permission.Name'
            }
        }
    }, {
        multiKey: true,
        route: 'componentfile',
        entityType: 'ComponentFile',
        resource: 'ComponentFiles',
        indirect: {
            component: {
                from: 'component',
                fromId: 'ComponentId',
                fromType: 'Component',
                to: 'file',
                toId: 'FileId',
                toProperty: 'File',
                toResource: 'Files',
                toExpand: null,
                template: '/app/partials/component/attachments.html',
                expand: 'File',
                sort: 'File.Name'
            },
            series: {
                from: 'series',
                fromId: 'ComponentId',
                fromType: 'Component',
                to: 'file',
                toId: 'FileId',
                toProperty: 'File',
                toResource: 'Files',
                toExpand: null,
                template: '/app/partials/component/attachments.html',
                expand: 'File',
                sort: 'File.Name'
            },
            file: {
                from: 'file',
                fromId: 'FileId',
                fromType: 'Files',
                to: 'component',
                toId: 'ComponentId',
                toProperty: 'Component',
                toResource: 'Components',
                toExpand: 'Category,Vendor',
                template: '/app/partials/file/components.html',
                sort: 'Component.Name',
                expand: 'Component'
            }
        }
    }, {
        multiKey: true,
        route: 'componentfile',
        entityType: 'ComponentFile',
        resource: 'ComponentFiles',
        indirect: {
            component: {
                from: 'component',
                fromId: 'ComponentId',
                fromType: 'Component',
                to: 'file',
                toId: 'FileId',
                toProperty: 'File',
                toResource: 'Files',
                toExpand: 'Vendor',
                template: '/app/partials/component/attachments.html',
                sort: 'File.Name',
                expand: 'File'
            },
            file: {
                from: 'file',
                fromId: 'FileId',
                fromType: 'Files',
                to: 'component',
                toId: 'ComponentId',
                toProperty: 'Component',
                toResource: 'Components',
                toExpand: 'Category,Vendor',
                template: '/app/partials/file/components.html',
                sort: 'Component.Name',
                expand: 'Component'
            }
        }
    }, {
        multiKey: true,
        route: 'componentmaintenance',
        entityType: 'ComponentMaintenance',
        resource: 'ComponentMaintenances',
        indirect: {
            component: {
                from: 'component',
                fromId: 'ComponentId',
                fromType: 'Component',
                to: 'maintenance',
                toId: 'MaintenanceId',
                toProperty: 'Maintenance',
                toResource: 'Maintenances',
                toExpand: 'Vendor',
                template: '/app/partials/component/maintenance.html',
                sort: 'Maintenance.Name',
                expand: 'Maintenance'
            },
            series: {
                from: 'series',
                fromId: 'ComponentId',
                fromType: 'Component',
                to: 'maintenance',
                toId: 'MaintenanceId',
                toProperty: 'Maintenance',
                toResource: 'Maintenances',
                toExpand: 'Vendor',
                template: '/app/partials/component/maintenance.html',
                sort: 'Maintenance.Name',
                expand: 'Maintenance'
            },
            maintenance: {
                from: 'maintenance',
                fromId: 'MaintenanceId',
                fromType: 'Maintenances',
                to: 'component',
                toId: 'ComponentId',
                toProperty: 'Component',
                toResource: 'Components',
                toExpand: 'Category,Vendor',
                template: '/app/partials/maintenance/components.html',
                sort: 'Component.Name',
                expand: 'Component'
            }
        }
    }, {
        multiKey: true,
        route: 'documentationchapter',
        entityType: 'DocumentationChapter',
        resource: 'DocumentationChapters',
        indirect: {
            chapter: {
                from: 'chapter',
                fromId: 'ChapterId',
                fromType: 'Chapter',
                to: 'documentation',
                toId: 'DocumentationId',
                toProperty: 'Documentation',
                toResource: 'Documentations',
                toExpand: 'Project',
                expand: 'Documentation,Documentation',
                template: '/app/partials/chapter/documentation.html',
                sort: 'Documentation.Name'
            },
            documentation: {
                from: 'documentation',
                fromId: 'DocumentationId',
                fromType: 'Documentation',
                to: 'abstractchapter',
                toId: 'ChapterId',
                toProperty: 'Chapter',
                toResource: 'AllChapters',
                toExpand: '',
                expand: 'Chapter',
                template: '/app/partials/documentation/chapters.html',
                sort: 'ChapterNumber'
            }
        }
    }, {
        multiKey: true,
        route: 'projectcomponent',
        entityType: 'ProjectComponent',
        resource: 'ProjectComponents',
        indirect: {
            project: {
                from: 'project',
                fromId: 'ProjectId',
                fromType: 'Project',
                to: 'abstractcomponent',
                toId: 'ComponentId',
                toProperty: 'Component',
                toResource: 'AllComponents',
                toExpand: 'Vendor',
                template: '/app/partials/project/components.html',
                expand: 'Component,Component.Vendor,Component.Category',
                sort: 'Component.Name'
            },
            component: {
                from: 'component',
                fromId: 'ComponentId',
                fromProperty: 'Component',
                to: 'project',
                toId: 'ProjectId',
                toProperty: 'Project',
                toResource: 'Projects',
                toExpand: 'Customer',
                template: '/app/partials/component/projects.html',
                expand: 'Project.Customer',
                sort: 'Project.ProjectNumber'
            },
            series: {
                from: 'series',
                fromId: 'ComponentId',
                fromProperty: 'Component',
                to: 'project',
                toId: 'ComponentId',
                toProperty: 'Project',
                toResource: 'Projects',
                toExpand: 'Customer',
                template: '/app/partials/component/projects.html',
                expand: 'Project.Customer',
                sort: 'Project.ProjectNumber'
            }
        }
    }, {
        multiKey: true,
        route: 'projectmaintenance',
        entityType: 'ProjectMaintenance',
        resource: 'ProjectMaintenances',
        indirect: {
            project: {
                from: 'project',
                fromId: 'ProjectId',
                fromType: 'Project',
                to: 'maintenance',
                toId: 'MaintenanceId',
                toProperty: 'Maintenance',
                toResource: 'Maintenances',
                toExpand: null,
                expand: 'Maintenance',
                template: '/app/partials/project/maintenance.html',
                sort: 'Maintenance.Name'
            },
            maintenance: {
                from: 'maintenance',
                fromId: 'MaintenanceId',
                fromType: 'Maintenance',
                to: 'project',
                toId: 'ProjectId',
                toProperty: 'Project',
                toResource: 'Projects',
                toExpand: 'Customer',
                expand: 'Project,Project.Customer',
                template: '/app/partials/maintenance/projects.html',
                sort: 'Project.ProjectNumber'
            }
        }
    }, {
        multiKey: true,
        route: 'projectsafety',
        entityType: 'ProjectSafety',
        resource: 'ProjectSafeties',
        expand: 'Safety',
        template: '/app/partials/project/safety.html',
        sort: 'Safety.Name',
        indirect: {
            project: {
                from: 'project',
                fromId: 'ProjectId',
                fromType: 'Project',
                to: 'safety',
                toId: 'SafetyId',
                toProperty: 'Safety',
                toResource: 'Safeties',
                toExpand: null,
            },
            safety: {
                from: 'safety',
                fromId: 'SafetyId',
                fromType: 'Safety',
                to: 'project',
                toId: 'ProjectId',
                toProperty: 'Project',
                toResource: 'Projects',
                toExpand: null,
                template: '/app/partials/safety/projects.html',
            }
        }
    }, {
        multiKey: true,
        route: 'componentsafety',
        entityType: 'ComponentSafety',
        resource: 'ComponentSafeties',
        expand: 'Safety',
        template: '/app/partials/project/safety.html',
        sort: 'Safety.Name',
        indirect: {
            component: {
                from: 'component',
                fromId: 'ComponentId',
                fromType: 'Component',
                to: 'safety',
                toId: 'SafetyId',
                toProperty: 'Safety',
                toResource: 'Safeties',
                toExpand: null,
            },
            series: {
                from: 'series',
                fromId: 'ComponentId',
                fromType: 'Component',
                to: 'safety',
                toId: 'SafetyId',
                toProperty: 'Safety',
                toResource: 'Safeties',
                toExpand: null,
            },
            safety: {
                from: 'safety',
                fromId: 'SafetyId',
                fromType: 'Safety',
                to: 'component',
                toId: 'ComponentId',
                toProperty: 'Component',
                toResource: 'Components',
                toExpand: 'Vendor',
                template: '/app/partials/safety/components.html',
                sort: 'Safety.Name'
            },
        }
    }, {
        multiKey: true,
        route: 'projectfile',
        entityType: 'ProjectFile',
        resource: 'ProjectFiles',
        indirect: {
            project: {
                from: 'project',
                fromId: 'ProjectId',
                fromType: 'Project',
                to: 'file',
                toId: 'FileId',
                toProperty: 'File',
                toResource: 'Files',
                toExpand: null,
                toRoute: '/folder/1',
                template: '/app/partials/project/attachments.html',
                expand: 'File,Vendor',
                sort: 'File.Name'
            },
            file: {
                from: 'file',
                fromId: 'FileId',
                fromType: 'File',
                to: 'project',
                toId: 'ProjectId',
                toProperty: 'Project',
                toResource: 'Projects',
                toExpand: null,
                template: '/app/partials/file/attachments.html',
                expand: 'Project,Vendor',
                sort: 'Project.ProjectNumber'
            }
        }
    }, {
        multiKey: true,
        route: 'chapterfile',
        entityType: 'ChapterFile',
        resource: 'ChapterFiles',
        indirect: {
            file: {
                from: 'file',
                fromId: 'FileId',
                fromType: 'File',
                to: 'chapter',
                toId: 'ChapterId',
                toProperty: 'Chapter',
                toResource: 'Chapters',
                toExpand: null,
                template: '/app/partials/file/chapters.html',
                expand: 'Chapter',
                sort: 'Chapter.Name'
            }
        }
    }];

    function copyProperties(type, originalEntity) {
        return _.findWhere(queryParameters, { route: type }).copy(originalEntity);
    }

    function settings() {
        return {
            copyProperties: copyProperties,
            defaultEntityValues: defaultEntityValues,
            detailViewSettings: detailViewSettings,
            getDefaultValuesForEntity: getDefaultValuesForEntity,
            queryParams: queryParameters,
            getResource: function (route) {
                return _.findWhere(queryParameters, { route: route }).resource;
            }
        };
    }
})();