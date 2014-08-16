(function () {
    'use strict';

    angular.module('app').factory('settings', function (utils) {

        var detailViewSettings = {

            tasks: {
                expand: 'Successors,Predecessors,ChildTasks,ParentTask,Resources.Employee,Project',
                hideToolbar: true,
                hideStatusbar: true,
                skipAudit: true,
                filter: 'ProjectId',
                rowClass: '{ "gantt-parent-row" : item.ChildTasks.length }',
                remote: true,
                local: true,
                load: false,
                columns: [
                    { name: 'Info', type: 'text', width: 7, readonly: true, formatter: 'getGanttInfo' },
                    { name: 'Name', type: 'text', width: 30, style: '{ "padding-left" :  (item.level * 16) + 4 }', colClass: '{ "gantt-parent-column" : item.ChildTasks.length, "gantt-child-column" : !item.ChildTasks.length }' },
                    { name: 'Duration', type: 'text', width: 10 },
                    { name: 'Start', property: 'StartDate', type: 'datetime', width: 25 },
                    { name: 'Finish', property: 'EndDate', type: 'datetime', width: 25 }
                ]
            },
            auditDetails: {
                columns: [
                    { name: 'Created', type: 'datetime', width: 8, readonly: true },
                    { name: 'Edited', type: 'datetime', width: 8, readonly: true },
                    { name: 'Creator', property: 'Creator.Name', type: 'text', width: 7, readonly: true, url: 'CreatorId', formatter: 'getUserInitials' },
                    { name: 'Editor', property: 'Editor.Name', type: 'text', width: 7, readonly: true, url: 'EditorId', formatter: 'getUserInitials' }
                ]
            },
            abstractchapter: {
                columns: [
				    { name: 'Name', type: 'text', width: 40, url: 'Id', readonly: true },
				    { name: 'Description', property: 'Description', type: 'text', width: 40, readonly: true },
				    { name: 'Project', property: 'Project.ProjectNumber', type: 'text', width: 30, readonly: true, url: 'ProjectId' }
                ]
            },
            projectcomponent: {
                project: {
                    columns: [
					    { name: 'Vendor', property: 'Component.Vendor.Name', type: 'text', width: 10, readonly: true, url: 'Component.VendorId' },
					    { name: 'Article', property: 'Component.Name', type: 'text', width: 15, link: 'abstractcomponent', linkProperty: 'Component', url: 'ComponentId' },
					    { name: 'Description', property: 'Component.Description', type: 'text', width: 25, readonly: true },
					    { name: 'Category', property: 'Component.Category.Name', type: 'text', width: 30, link: 'category', linkProperty: 'Category', basePermission: 'category', url: 'Component.CategoryId' },
					    { name: 'Info', type: 'textarea', width: 10 },
					    { name: 'Count', type: 'number', width: 5 },
					    { name: 'Unit', type: 'enumeration', enumtype: 'Units', width: 5 },
					    { name: 'Spare', property: 'SpareParts', type: 'number', width: 7 },
					    { name: 'Manual', property: 'IncludeInManual', type: 'bool', width: 7 }
                    ]
                },
                file: {
                    columns: [
						{ name: 'Project', property: 'Project.ProjectNumber', type: 'text', width: 10, url: 'ProjectId', readonly: true },
						{ name: 'Description', property: 'Project.Name', type: 'text', width: 60, readonly: true },
						{ name: 'Customer', property: 'Project.Customer.Name', type: 'text', width: 10, readonly: true }
                    ]
                },
                component: {
                    columns: [
					    { name: 'Project', property: 'Project.ProjectNumber', type: 'text', width: 10, url: 'ProjectId', readonly: true },
					    { name: 'Description', property: 'Project.Name', type: 'text', width: 30, readonly: true },
					    { name: 'Customer', property: 'Project.Customer.Name', type: 'text', width: 20, url: 'CustomerId', readonly: true },
					    { name: 'Info', type: 'text', width: 25, readonly: true },
					    { name: 'Count', type: 'number', width: 5, readonly: true }
                    ]
                },
                componentseries: {
                    columns: [
					    { name: 'Project', property: 'Project.ProjectNumber', type: 'text', width: 10, url: 'ProjectId', readonly: true },
					    { name: 'Description', property: 'Project.Name', type: 'text', width: 30, readonly: true },
					    { name: 'Customer', property: 'Project.Customer.Name', type: 'text', width: 20, url: 'CustomerId', readonly: true },
					    { name: 'Info', type: 'text', width: 25, readonly: true },
					    { name: 'Count', type: 'number', width: 5, readonly: true }
                    ]
                }
            },
            projectfile: {
                project: {
                    columns: [
					    { name: 'File', property: 'File.Name', type: 'text', width: 20, link: 'file', linkProperty: 'File', readonly: true, url: 'FileId' },
					    { name: 'Name', type: 'text', width: 20 },
					    { name: 'Role', type: 'enumeration', enumtype: 'FileRoles', width: 14 },
					    { name: 'Vendor', property: 'Vendor.Name', type: 'text', width: 30, link: 'vendor', linkProperty: 'Vendor', basePermission: true, url: 'VendorId' },
					    { name: 'Manual', property: 'IncludeInManual', type: 'bool', width: 8 },
					    { name: 'Printed', property: 'IncludedPrintedVersion', type: 'bool', width: 8 }
                    ]
                },
                file: {
                    columns: [
					    { name: 'Project', property: 'Project.ProjectNumber', type: 'text', width: 10, url: 'ProjectId', readonly: true },
					    { name: 'Description', property: 'Project.Name', type: 'text', width: 60, readonly: true },
					    { name: 'Customer', property: 'Project.Customer.Name', type: 'text', width: 10, readonly: true },
					    { name: 'Role', type: 'enumeration', enumtype: 'FileRoles', width: 20, readonly: true }
                    ]
                }
            },
            projectmaintenance: {
                project: {
                    columns: [
					    { name: 'Name', property: 'Maintenance.Name', type: 'text', width: 80, url: 'MaintenanceId', readonly: true },
					    { name: 'Manual', property: 'IncludeInManual', type: 'bool', width: 10 },
					    { name: 'Remarks', type: 'text', width: 10 }
                    ]
                },
                maintenance: {
                    columns: [
					    { name: 'Project', property: 'Project.ProjectNumber', type: 'text', width: 20, url: 'ProjectId', readonly: true },
					    { name: 'Description', property: 'Project.Name', type: 'text', width: 60, readonly: true },
					    { name: 'Customer', property: 'Project.Customer.Name', type: 'text', width: 20, readonly: true }
                    ]
                }
            },
            projectsafety: {
                project: {
                    columns: [
					    { name: 'Name', property: 'Safety.Name', type: 'text', width: 40, url: 'SafetyId', readonly: true },
					    { name: 'Location', type: 'text', width: 40 },
					    { name: 'Role', type: 'enumeration', enumtype: 'SafetyRoles', width: 10 },
					    { name: 'Manual', property: 'IncludeInManual', type: 'bool', width: 10 }
                    ]
                },
                safety: {
                    columns: [
					    { name: 'Project', property: 'Project.ProjectNumber', type: 'text', width: 10, url: 'ProjectId', readonly: true },
					    { name: 'Description', property: 'Project.Name', type: 'text', width: 60, readonly: true },
					    { name: 'Customer', property: 'Project.Customer.Name', type: 'text', width: 10, readonly: true }
                    ]
                }
            },
            project: {
                from: 'Projects',
                expand: 'Customer,Creator,Editor,ProjectManager',
                copyExpand: 'Customer,ProjectManager,Documentation,Maintenance,Safety',
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
                columns: [
				    { name: 'Project', property: 'ProjectNumber', type: 'text', width: 7, url: 'Id' },
				    { name: 'Description', property: 'Name', type: 'text', width: 35 },
				    { name: 'Customer', property: 'Customer.Name', type: 'text', width: 20, link: 'customer', linkProperty: 'Customer', basePermission: true, url: 'CustomerId' },
				    { name: 'Status', type: 'enumeration', enumtype: 'ProjectStatus', width: 8 }
                ]
            },
            orderconfirmation: {
                from: 'OrderConfirmations',
                expand: 'Creator,Editor,Project,Vendor,CustomerReference',
                tabs: [
				    { title: 'Properties', template: '/app/partials/orderconfirmation/properties.html' },
				    { title: 'Items', type: 'orderconfirmationitem' }
                ],
                skipAudit: true,
                columns: [
				    { name: 'Name', type: 'text', width: 7, url: 'Id' },
				    { name: 'Description', type: 'text', width: 35 },
				    { name: 'Vendor', property: 'Vendor.Name', type: 'text', width: 20, link: 'customer', linkProperty: 'Customer', basePermission: true, url: 'CustomerId' }
                ]
            },
            orderconfirmationitem: {
                skipAudit: true,
                columns: [
				    { name: 'Article Number', property: 'Component.Name', type: 'text', width: 10, url: 'ComponentId', readonly: true },
				    { name: 'Description', property: 'Component.Description', type: 'text', width: 20, readonly: true },
				    { name: 'Order Details', property: 'OrderSpecificDescription', type: 'text', width: 20 },
				    { name: 'Quantity', type: 'number', width: 10 },
				    { name: 'Unit', type: 'enumeration', enumtype: 'Units', width: 10 },
				    { name: 'Unit price', property: 'AmountPerUnit', type: 'number', width: 10 },
				    { name: 'Discount', type: 'number', width: 10 },
				    { name: 'Total', property: 'TotalAmount', type: 'number', width: 10 }
                ]
            },
            experience: {
                from: 'Experiences',
                expand: 'Creator,Editor,Project,Component',
                tabs: [
				    { title: 'Properties', template: '/app/partials/experience/properties.html' }
                ],
                columns: [
				    { name: 'Name', type: 'text', width: 36, url: 'Id' },
				    { name: 'Tags', type: 'text', width: 36 }
                ]
            },
            file: {
                from: 'Files',
                expand: 'Creator,Editor,Folder,Folder.Parent,Folder.Parent.Parent,Folder.Parent.Parent.Parent,Folder.Parent.Parent.Parent.Parent,Folder.Parent.Parent.Parent.Parent.Parent',
                tabs: [
				    { title: 'Projects', type: 'projectfile' },
				    { title: 'Components', type: 'componentfile' },
				    { title: 'Documentation', type: 'documentation' },
				    { title: 'Maintenance', type: 'maintenance' },
				    { title: 'Safety', type: 'safety' },
				    { title: 'Chapters', type: 'chapterfile' }
                ]
            },
            folder: {
                columns: [
				    { name: 'Filename', property: 'Name', type: 'text', width: 36, url: 'Id' },
				    { name: 'Type', type: 'text', readonly: true, width: 8 },
				    { name: 'Description', type: 'text', width: 36 },
				    { name: 'Size', property: 'FileSize', type: 'number', width: 10, readonly: true, formatter: 'getFormattedSize' },
				    { name: 'Overwritable', property: 'IsOverwritable', type: 'bool', width: 10 }
                ]
            },
            employee: {
                from: 'Employees',
                expand: 'Account,ManagedProjects,Tasks',
                tabs: [
				    { title: 'Properties', template: '/app/partials/employee/properties.html' },
				    { title: 'Managed Projects', type: 'project' },
				    { title: 'Tasks', type: 'employeetasks' }
                ],
                columns: [
				    { name: 'Name', type: 'text', width: 70, url: 'Id' },
				    { name: 'Title', type: 'text', width: 30 }
                ]
            },
            category: {
                from: 'Categories',
                expand: 'Creator,Editor',
                tabs: [
				    { title: 'Components', type: 'component' }
                ],
                columns: [
				    { name: 'Name', type: 'text', width: 23, url: 'Id' },
				    { name: 'Description', type: 'text', width: 40 },
				    { name: 'Vendor', property: 'Vendor.Name', type: 'text', width: 15, link: 'vendor', linkProperty: 'Vendor', basePermission: true, url: 'VendorId' },
				    { name: 'Category', property: 'Category.Name', type: 'text', width: 20, link: 'category', linkProperty: 'Category', basePermission: true, url: 'CategoryId' }
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
                columns: [
				    { name: 'Name', type: 'text', width: 40, url: 'Id' },
				    { name: 'Description', property: 'Description', type: 'text', width: 40 },
				    { name: 'Project', property: 'Project.ProjectNumber', type: 'text', width: 30, readonly: true, url: 'ProjectId' }
                ]
            },
            customer: {
                from: 'Customers',
                expand: 'Creator,Editor',
                tabs: [{ title: 'Projects', type: 'project', }],
                columns: [
				    { name: 'Name', type: 'text', width: 100, url: 'Id' }
                ]
            },
            abstractcomponent: {
                columns: [
				    { name: 'Name', type: 'text', width: 23, url: 'Id' },
				    { name: 'Description', type: 'text', width: 40 },
				    { name: 'Vendor', property: 'Vendor.Name', type: 'text', width: 15, link: 'vendor', linkProperty: 'Vendor', basePermission: true, url: 'VendorId' },
				    { name: 'Category', property: 'Category.Name', type: 'text', width: 20, link: 'category', linkProperty: 'Category', basePermission: true, url: 'CategoryId' }
                ]
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
                ],
                component: {
                    columns: [
					    { name: 'Name', type: 'text', width: 23, url: 'Id' },
					    { name: 'Description', type: 'textarea', width: 40 },
					    { name: 'Vendor', property: 'Vendor.Name', type: 'text', width: 15, link: 'vendor', linkProperty: 'Vendor', basePermission: true, url: 'VendorId' },
					    { name: 'Category', property: 'Category.Name', type: 'text', width: 20, link: 'category', linkProperty: 'Category', basePermission: true, url: 'CategoryId' }
                    ]
                },
                vendor: {
                    columns: [
					    { name: 'Name', type: 'text', width: 23, url: 'Id' },
					    { name: 'Description', type: 'textarea', width: 40 },
					    { name: 'Vendor', property: 'Vendor.Name', type: 'text', width: 15, link: 'vendor', linkProperty: 'Vendor', basePermission: true, url: 'VendorId' },
					    { name: 'Category', property: 'Category.Name', type: 'text', width: 20, link: 'category', linkProperty: 'Category', basePermission: true, url: 'CategoryId' }
                    ]
                }
            },
            documentation: {
                from: 'Documentations',
                expand: 'Project,Creator,Editor,Image,Image.Folder,Image.Folder.Parent,Image.Folder.Parent.Parent,Image.Folder.Parent.Parent.Parent',
                tabs: [
				    { title: 'Properties', template: '/app/partials/documentation/properties.html' },
				    { title: 'Chapters', type: 'documentationchapter' }
                ],
                columns: [
				    { name: 'Name', type: 'text', width: 70, url: 'Id' },
				    { name: 'Project', property: 'Project.ProjectNumber', type: 'text', width: 30, link: 'project', linkProperty: 'Project', url: 'ProjectId' }
                ]
            },
            documentationchapter: {
                documentation: {
                    columns: [
					    { name: '#', property: 'ChapterNumber', type: 'text', width: 5 },
					    { name: 'Name', property: 'Chapter.Name', type: 'text', width: 20, link: 'chapter', linkProperty: 'Chapter', url: 'ChapterId' },
					    { name: 'Alternate title', property: 'Title', type: 'text', width: 15 },
					    { name: 'Description', property: 'Chapter.Description', type: 'text', width: 25, readonly: true },
					    { name: 'Page break', property: 'NewPage', type: 'bool', width: 7 },
					    { name: 'Parameters', type: 'text', width: 18 },
					    { name: 'Project', property: 'Chapter.Project.ProjectNumber', readonly: true, type: 'text', width: 10, url: 'Chapter.ProjectId' }
                    ]
                },
                chapter: {
                    columns: [
					    { name: 'Name', property: 'Documentation.Name', type: 'text', width: 70, url: 'Id' },
					    { name: 'Project', property: 'Documentation.Project.ProjectNumber', type: 'text', width: 30, link: 'project', linkProperty: 'Project', url: 'ProjectId' }
                    ]
                }
            },
            maintenance: {
                from: 'Maintenances',
                expand: 'Vendor,Creator,Editor,Manual,Image.Folder.Parent.Parent,Image.Folder.Parent.Parent.Parent',
                tabs: [
				    { title: 'Properties', template: '/app/partials/maintenance/properties.html' },
				    { title: 'Components', type: 'componentmaintenance' },
				    { title: 'Project', type: 'projectmaintenance' }
                ],
                columns: [
				    { name: 'Name', type: 'text', width: 38, url: 'Id' },
				    { name: 'Vendor', property: 'Vendor.Name', type: 'text', width: 20, link: 'vendor', linkProperty: 'Vendor', basePermission: true, url: 'VendorId' },
				    { name: 'Day', property: 'IntervalDaily', type: 'bool', width: 5 },
				    { name: 'Week', property: 'IntervalWeekly', type: 'bool', width: 5 },
				    { name: '2. week', property: 'IntervalWeekly2', type: 'bool', width: 5 },
				    { name: 'Month', property: 'IntervalMonthly', type: 'bool', width: 5 },
				    { name: '3. month', property: 'IntervalMonthly3', type: 'bool', width: 5 },
				    { name: '6. month', property: 'IntervalHalfYearly', type: 'bool', width: 5 },
				    { name: 'Year', property: 'IntervalYearly', type: 'bool', width: 5 },
				    { name: '>Year', property: 'IntervalRareky', type: 'bool', width: 5 }
                ]
            },
            componentmaintenance: {
                maintenance: {
                    columns: [
					    { name: 'Vendor', property: 'Component.Vendor.Name', type: 'text', width: 20, url: 'Component.VendorId', readonly: true },
					    { name: 'Name', property: 'Component.Name', type: 'text', width: 40, url: 'ComponentId', readonly: true },
					    { name: 'Description', property: 'Component.Description', type: 'text', width: 40, readonly: true }
                    ]
                },
                component: {
                    columns: [
					    { name: 'Name', property: 'Maintenance.Name', type: 'text', width: 25, url: 'MaintenanceId', readonly: true },
					    { name: 'Remarks', type: 'text', width: 30 },
					    { name: 'Day', property: 'Maintenance.IntervalDaily', type: 'bool', width: 5 },
					    { name: 'Week', property: 'Maintenance.IntervalWeekly', type: 'bool', width: 5 },
					    { name: '2. week', property: 'Maintenance.IntervalWeekly2', type: 'bool', width: 5 },
					    { name: 'Month', property: 'Maintenance.IntervalMonthly', type: 'bool', width: 5 },
					    { name: '3. month', property: 'Maintenance.IntervalMonthly3', type: 'bool', width: 5 },
					    { name: '6. month', property: 'Maintenance.IntervalHalfYearly', type: 'bool', width: 5 },
					    { name: 'Year', property: 'Maintenance.IntervalYearly', type: 'bool', width: 5 },
					    { name: '>Year', property: 'Maintenance.IntervalRarely', type: 'bool', width: 5 },
                    ]
                },
                componentseries: {
                    columns: [
					    { name: 'Name', property: 'Maintenance.Name', type: 'text', width: 25, url: 'MaintenanceId', readonly: true },
					    { name: 'Remarks', type: 'text', width: 30 },
					    { name: 'Day', property: 'Maintenance.IntervalDaily', type: 'bool', width: 5 },
					    { name: 'Week', property: 'Maintenance.IntervalWeekly', type: 'bool', width: 5 },
					    { name: '2. week', property: 'Maintenance.IntervalWeekly2', type: 'bool', width: 5 },
					    { name: 'Month', property: 'Maintenance.IntervalMonthly', type: 'bool', width: 5 },
					    { name: '3. month', property: 'Maintenance.IntervalMonthly3', type: 'bool', width: 5 },
					    { name: '6. month', property: 'Maintenance.IntervalHalfYearly', type: 'bool', width: 5 },
					    { name: 'Year', property: 'Maintenance.IntervalYearly', type: 'bool', width: 5 },
					    { name: '>Year', property: 'Maintenance.IntervalRarely', type: 'bool', width: 5 },
                    ]
                }
            },
            componentfile: {
                component: {
                    columns: [
					    { name: 'File', property: 'File.Name', type: 'text', width: 40, link: 'file', linkProperty: 'File', readonly: true, url: 'FileId' },
					    { name: 'Role', type: 'enumeration', enumtype: 'FileRoles', width: 10 },
					    { name: 'Vendor', property: 'Vendor.Name', type: 'text', width: 30, link: 'vendor', linkProperty: 'Vendor', basePermission: true, url: 'VendorId' },
					    { name: 'Manual', property: 'IncludeInManual', type: 'bool', width: 10 },
					    { name: 'Printed', property: 'IncludedPrintedVersion', type: 'bool', width: 10 }
                    ]
                },
                componentseries: {
                    columns: [
					    { name: 'File', property: 'File.Name', type: 'text', width: 40, link: 'file', linkProperty: 'File', readonly: true, url: 'FileId' },
					    { name: 'Role', type: 'enumeration', enumtype: 'FileRoles', width: 10 },
					    { name: 'Vendor', property: 'Vendor.Name', type: 'text', width: 30, link: 'vendor', linkProperty: 'Vendor', basePermission: true, url: 'VendorId' },
					    { name: 'Manual', property: 'IncludeInManual', type: 'bool', width: 10 },
					    { name: 'Printed', property: 'IncludedPrintedVersion', type: 'bool', width: 10 }
                    ]
                },
                file: {
                    columns: [
					    { name: 'Vendor', property: 'Component.Vendor.Name', type: 'text', width: 10, readonly: true, url: 'Component.VendorId' },
					    { name: 'Article', property: 'Component.Name', type: 'text', width: 15, link: 'abstractcomponent', linkProperty: 'Component', url: 'ComponentId' },
					    { name: 'Description', property: 'Component.Description', type: 'text', width: 25, readonly: true },
					    { name: 'Category', property: 'Component.Category.Name', type: 'text', width: 30, link: 'category', linkProperty: 'Category', basePermission: 'category', url: 'Component.CategoryId' },
					    { name: 'Role', type: 'enumeration', enumtype: 'FileRoles', width: 10 },
					    { name: 'Manual', property: 'IncludeInManual', type: 'bool', width: 10 },
					    { name: 'Printed', property: 'IncludedPrintedVersion', type: 'bool', width: 10 }
                    ]
                }
            },
            role: {
                from: 'Roles',
                expand: 'Creator,Editor',
                tabs: [
				    { title: 'Permissions', type: 'rolepermission' },
				    { title: 'Users', type: 'userrole' }
                ],
                columns: [
				    { name: 'Name', type: 'text', width: 100, url: 'Id' }
                ]
            },
            rolepermission: {
                columns: [
				    { name: 'Permission', property: 'Permission.Info', type: 'text', width: 60, readonly: true },
				    { name: 'View', property: 'Read', type: 'enumeration', enumtype: 'RoleScopes', width: 10 },
				    { name: 'Create', property: 'Create', type: 'enumeration', enumtype: 'RoleScopes', width: 10 },
				    { name: 'Modify', property: 'Update', type: 'enumeration', enumtype: 'RoleScopes', width: 10 },
				    { name: 'Delete', property: 'Delete', type: 'enumeration', enumtype: 'RoleScopes', width: 10 }
                ]
            },
            safety: {
                from: 'Safeties',
                expand: 'Creator,Editor,Image,Image.Folder,Image.Folder.Parent,Image.Folder.Parent.Parent,Image.Folder.Parent.Parent.Parent',
                tabs: [
				    { title: 'Properties', template: '/app/partials/safety/properties.html' },
				    { title: 'Components', type: 'componentsafety' },
				    { title: 'Projects', type: 'projectsafety' }
                ],
                columns: [
				    { name: 'Name', type: 'text', width: 100, url: 'Id' }
                ]
            },
            componentsafety: {
                safety: {
                    columns: [
					    { name: 'Vendor', property: 'Component.Vendor.Name', type: 'text', width: 20, url: 'Component.VendorId', readonly: true },
					    { name: 'Name', property: 'Component.Name', type: 'text', width: 40, url: 'ComponentId', readonly: true },
					    { name: 'Description', property: 'Component.Description', type: 'text', width: 40, readonly: true }
                    ]
                },
                component: {
                    columns: [
					    { name: 'name', property: 'safety.name', type: 'text', width: 40, url: 'safetyid', readonly: true },
					    { name: 'location', type: 'text', width: 40 },
					    { name: 'role', type: 'enumeration', enumtype: 'safetyroles', width: 10 },
					    { name: 'manual', property: 'includeinmanual', type: 'bool', width: 10 }
                    ]
                },
                componentseries: {
                    columns: [
					    { name: 'name', property: 'safety.name', type: 'text', width: 40, url: 'safetyid', readonly: true },
					    { name: 'location', type: 'text', width: 40 },
					    { name: 'role', type: 'enumeration', enumtype: 'safetyroles', width: 10 },
					    { name: 'manual', property: 'includeinmanual', type: 'bool', width: 10 }
                    ]
                }
            },
            componentseries: {
                from: 'ComponentSeries',
                expand: 'Vendor,Category,Creator,Editor',
                tabs: [
				    { title: 'Properties', template: '/app/partials/componentseries/properties.html' },
				    { title: 'Attachments', type: 'componentfile' },
				    { title: 'Maintenance', type: 'componentmaintenance' },
				    { title: 'Safety', type: 'componentsafety' },
				    { title: 'Project', type: 'projectcomponent' }
                ],
                columns: [
				    { name: 'Name', type: 'text', width: 30, url: 'Id' },
				    { name: 'Description', type: 'text', width: 40 },
				    { name: 'Vendor', property: 'Vendor.Name', type: 'text', width: 30, link: 'vendor', linkProperty: 'Vendor', basePermission: true }
                ]
            },
            user: {
                from: 'Users',
                tabs: [
				    { title: 'Properties', template: '/app/partials/user/properties.html' },
				    { title: 'Roles', type: 'userrole' }
                ],
                columns: [
				    { name: 'Name', type: 'text', width: 40, url: 'Id' },
				    { name: 'Username', type: 'text', width: 30 },
				    { name: 'Email', type: 'text', width: 30 }
                ]
            },
            userrole: {
                role: {
                    columns: [
					    { name: 'User', property: 'User.Name', type: 'text', width: 100, url: 'UserId', readonly: true }
                    ]
                },
                user: {
                    columns: [
					    { name: 'Name', property: 'Role.Name', type: 'text', width: 100, url: 'RoleId', readonly: true }
                    ]
                }
            },
            vendor: {
                from: 'Vendors',
                expand: 'Creator,Editor',
                tabs: [
				    { title: 'Properties', template: '/app/partials/vendor/properties.html', },
				    { title: 'Components', type: 'component' }
                ],
                columns: [
				    { name: 'Name', type: 'text', width: 70, url: 'Id' },
				    { name: 'Abbrevation', property: 'ShortName', type: 'text', width: 30 }
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
            DocumentationChapter: { UniqueKey: utils.generateUid, ChapterNumber: '1' },
            Employee: { Name: 'New employee' },
            Experience: { Name: 'New experience' },
            Folder: { Name: 'New folder' },
            Maintenance: { Name: 'New maintenance' },
            Permission: { Name: 'New permission' },
            Project: { ProjectNumber: 'P' + (new Date().getFullYear()) + '-???', Name: 'New project', Status: 'Planned' },
            ProjectFile: { Name: 'New attachment', Role: 'Other', IncludeInManual: true },
            ProjectComponent: { Count: 1, IncludeInManual: true, Unit: 'Units' },
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

        var queryParameters = [
		    {
		        route: 'project',
		        entityType: 'Project',
		        resource: 'Projects',
		        expand: 'Customer,Creator,Editor',
		        sort: 'ProjectNumber',
		        duplicate: 'CustomerId,ProjectManagerId,ProjectNumber,Status,Name',
		        duplicateExpand: [
			    {
			        name: 'Components',
			        properties: 'ComponentId,Count,Unit,SpareParts,IncludeInManual,FileId,Info'
			    },
			    {
			        name: 'Maintenances',
			        properties: 'MaintenanceId,Remarks,IncludeInManual'
			    },
			    {
			        name: 'Safeties',
			        properties: 'SafetyId,IncludeInManual,Role,Location'
			    }
		        ],
		        duplicateId: 'ProjectId',
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
		        route: 'tasks',
		        entityType: 'Task',
		        resource: 'Tasks',
		        load: true
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
		                hierarchy: false,
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
		        duplicate: 'Name,NameSe,Content,ContentSe,Description,DescriptionSe,ProjectId',
		        duplicateExpand: [
			    {
			        name: 'Files',
			        properties: 'FileId'
			    }
		        ],
		        duplicateId: 'ChapterId',
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
		        duplicate: 'Name,NameSe,Description,ImageId,ProjectId',
		        duplicateExpand: [
			    {
			        name: 'DocumentationChapters',
			        properties: 'ChapterId,FileId,ChapterNumber,Title,Description,PageBreak,Parameters'
			    }
		        ],
		        duplicateId: 'DocumentationId',
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
		                Name: 'Copy of ' + originalEntity.Name
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
		        route: 'componentseries',
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
		                toRoute: '/folder/1',
		                expand: 'File',
		                sort: 'File.Name'
		            },
		            componentseries: {
		                from: 'series',
		                fromId: 'ComponentId',
		                fromType: 'Component',
		                to: 'file',
		                toId: 'FileId',
		                toProperty: 'File',
		                toResource: 'Files',
		                toExpand: null,
		                toRoute: '/folder/1',
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
		                sort: 'Component.Name',
		                expand: 'Component,Vendor'
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
		                sort: 'Maintenance.Name',
		                expand: 'Maintenance'
		            },
		            componentseries: {
		                from: 'series',
		                fromId: 'ComponentId',
		                fromType: 'Component',
		                to: 'maintenance',
		                toId: 'MaintenanceId',
		                toProperty: 'Maintenance',
		                toResource: 'Maintenances',
		                toExpand: 'Vendor',
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
		                expand: 'Project.Customer',
		                sort: 'Project.ProjectNumber'
		            },
		            componentseries: {
		                from: 'series',
		                fromId: 'ComponentId',
		                fromProperty: 'Component',
		                to: 'project',
		                toId: 'ComponentId',
		                toProperty: 'Project',
		                toResource: 'Projects',
		                toExpand: 'Customer',
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
		                sort: 'Project.ProjectNumber'
		            }
		        }
		    }, {
		        multiKey: true,
		        route: 'projectsafety',
		        entityType: 'ProjectSafety',
		        resource: 'ProjectSafeties',
		        expand: 'Safety,Project',
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
		            }
		        }
		    }, {
		        multiKey: true,
		        route: 'componentsafety',
		        entityType: 'ComponentSafety',
		        resource: 'ComponentSafeties',
		        expand: 'Safety,Component,Component.Vendor',
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
		            componentseries: {
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
		                expand: 'Chapter',
		                sort: 'Chapter.Name'
		            }
		        }
		    }
        ];

        function copyProperties(type, originalEntity) {
            return _.findWhere(queryParameters, { route: type }).copy(originalEntity);
        }

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
    });
})();