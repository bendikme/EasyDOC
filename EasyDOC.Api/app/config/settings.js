(function() {
    'use strict';

    angular.module('app').factory('settings', function(utils) {

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
                    { name: 'Info', type: 'text', width: 6, readonly: true, formatter: 'getGanttInfo' },
                    { name: 'Navn', property: 'Name', type: 'text', width: 30, colStyle: '{ "padding-left" :  (item.level * 16) + "px" }', colClass: '{ "gantt-parent-column" : item.ChildTasks.length, "gantt-child-column" : !item.ChildTasks.length }' },
                    { name: 'Varighet', property: 'Duration', type: 'text', width: 15 },
                    { name: 'Start', property: 'StartDate', type: 'datetime', width: 15 },
                    { name: 'Ferdig', property: 'EndDate', type: 'datetime', width: 15 }
                ]
            },
            auditDetails: {
                columns: [
                    { name: 'Opprettet', property: 'Created', type: 'datetime', width: 8, readonly: true, visible: false },
                    { name: 'Endret', property: 'Edited', type: 'datetime', width: 8, readonly: true, visible: false },
                    { name: 'Opprettet av', property: 'Creator.Name', type: 'text', width: 7, readonly: true, url: 'CreatorId', formatter: 'getUserInitials', visible: false },
                    { name: 'Endret av', property: 'Editor.Name', type: 'text', width: 7, readonly: true, url: 'EditorId', formatter: 'getUserInitials', visible: false }
                ]
            },
            abstractchapter: {
                columns: [
                    { name: 'Navn', property: 'Name', type: 'text', width: 40, url: 'Id', readonly: true },
                    { name: 'Beskrivelse', property: 'Description', type: 'text', width: 40, readonly: true },
                    { name: 'Prosjekt', property: 'Project.ProjectNumber', type: 'text', width: 30, readonly: true, url: 'ProjectId' }
                ]
            },
            projectcomponent: {
                project: {
                    columns: [
                        { name: 'Leverandør', property: 'Component.Vendor.Name', type: 'text', width: 10, readonly: true, url: 'Component.VendorId' },
                        { name: 'Artikkel', property: 'Component.Name', type: 'text', width: 15, link: 'abstractcomponent', linkProperty: 'Component', url: 'ComponentId', readonly: true },
                        { name: 'Beskrivelse', property: 'Component.Description', type: 'text', width: 25, readonly: true },
                        { name: 'Kategori', property: 'Component.Category.Name', type: 'relation', width: 15, link: 'category', linkProperty: 'Component.Category', resource: 'Categories', basePermission: 'category', url: 'Component.CategoryId' },
                        { name: 'Info', type: 'textarea', width: 20 },
                        { name: 'Antall', property: 'Count', type: 'number', width: 5 },
                        { name: 'Enhet', property: 'Unit', type: 'enumeration', enumtype: 'Units', width: 5 },
                        { name: 'Reserve', property: 'SpareParts', type: 'number', width: 7 },
                        { name: 'Manual', property: 'IncludeInManual', type: 'bool', width: 7 }
                    ]
                },
                file: {
                    columns: [
                        { name: 'Prosjekt', property: 'Project.ProjectNumber', type: 'text', width: 10, url: 'ProjectId', readonly: true },
                        { name: 'Beskrivelse', property: 'Project.Name', type: 'text', width: 60, readonly: true },
                        { name: 'Kunde', property: 'Project.Customer.Name', type: 'text', width: 10, readonly: true }
                    ]
                },
                component: {
                    columns: [
                        { name: 'Prosjekt', property: 'Project.ProjectNumber', type: 'text', width: 10, url: 'ProjectId', readonly: true },
                        { name: 'Beskrivelse', property: 'Project.Name', type: 'text', width: 30, readonly: true },
                        { name: 'Kunde', property: 'Project.Customer.Name', type: 'text', width: 20, url: 'CustomerId', readonly: true },
                        { name: 'Info', type: 'text', width: 25, readonly: true },
                        { name: 'Antall', property: 'Count', type: 'number', width: 5, readonly: true }
                    ]
                },
                componentseries: {
                    columns: [
                        { name: 'Prosjekt', property: 'Project.ProjectNumber', type: 'text', width: 10, url: 'ProjectId', readonly: true },
                        { name: 'Beskrivelse', property: 'Project.Name', type: 'text', width: 30, readonly: true },
                        { name: 'Kunde', property: 'Project.Customer.Name', type: 'text', width: 20, url: 'CustomerId', readonly: true },
                        { name: 'Info', type: 'text', width: 25, readonly: true },
                        { name: 'Antall', property: 'Count', type: 'number', width: 5, readonly: true }
                    ]
                }
            },
            projectfile: {
                project: {
                    columns: [
                        { name: 'Fil', property: 'File.Name', type: 'text', width: 20, link: 'file', linkProperty: 'File', readonly: true, url: 'FileId' },
                        { name: 'Navn', property: 'Name', type: 'text', width: 20 },
                        { name: 'Rolle', property: 'Role', type: 'enumeration', enumtype: 'FileRoles', width: 14 },
                        { name: 'Leverandør', property: 'Vendor.Name', type: 'relation', resource: 'Vendors', width: 30, link: 'vendor', linkProperty: 'Vendor', basePermission: true, url: 'VendorId' },
                        { name: 'Manual', property: 'IncludeInManual', type: 'bool', width: 8 },
                        { name: 'Utskrift', property: 'IncludedPrintedVersion', type: 'bool', width: 8 }
                    ]
                },
                file: {
                    columns: [
                        { name: 'Prosjekt', property: 'Project.ProjectNumber', type: 'text', width: 10, url: 'ProjectId', readonly: true },
                        { name: 'Beskrivelse', property: 'Project.Name', type: 'text', width: 60, readonly: true },
                        { name: 'Kunde', property: 'Project.Customer.Name', type: 'text', width: 10, readonly: true },
                        { name: 'Rolle', property: 'Role', type: 'enumeration', enumtype: 'FileRoles', width: 20, readonly: true }
                    ]
                }
            },
            projectmaintenance: {
                project: {
                    columns: [
                        { name: 'Navn', property: 'Maintenance.Name', type: 'text', width: 80, url: 'MaintenanceId', readonly: true },
                        { name: 'Manual', property: 'IncludeInManual', type: 'bool', width: 10 },
                        { name: 'Merknader', property: 'Remarks', type: 'text', width: 10 }
                    ]
                },
                maintenance: {
                    columns: [
                        { name: 'Prosject', property: 'Project.ProjectNumber', type: 'text', width: 20, url: 'ProjectId', readonly: true },
                        { name: 'Beskrivelse', property: 'Project.Name', type: 'text', width: 60, readonly: true },
                        { name: 'Kunde', property: 'Project.Customer.Name', type: 'text', width: 20, readonly: true }
                    ]
                }
            },
            projectsafety: {
                project: {
                    columns: [
                        { name: 'Navn', property: 'Safety.Name', type: 'text', width: 40, url: 'SafetyId', readonly: true },
                        { name: 'Sted', property: 'Location', type: 'text', width: 40 },
                        { name: 'Rolle', property: 'Role', type: 'enumeration', enumtype: 'SafetyRoles', width: 10 },
                        { name: 'Manual', property: 'IncludeInManual', type: 'bool', width: 10 }
                    ]
                },
                safety: {
                    columns: [
                        { name: 'Prosjekt', property: 'Project.ProjectNumber', type: 'text', width: 10, url: 'ProjectId', readonly: true },
                        { name: 'Beskrivelse', property: 'Project.Name', type: 'text', width: 60, readonly: true },
                        { name: 'Kunde', property: 'Project.Customer.Name', type: 'text', width: 10, readonly: true }
                    ]
                }
            },
            project: {
                from: 'Projects',
                expand: 'Customer,Creator,Editor,ProjectManager',
                copyExpand: 'Customer,ProjectManager,Documentation,Maintenance,Safety',
                tabs: [
                    {
                        title: 'Egenskaper',
                        sections: [
                            {
                                title: 'Informasjon',
                                fields: [
                                    { title: 'Prosjektnummer', type: 'text', property: 'ProjectNumber', placeholder: 'Velg et prosjektnummer' },
                                    { title: 'Kunde', type: 'async', id: 'CustomerId', url: 'customer', property: 'Customer', resource: 'Customers', placeholder: 'Velg en kunde' },
                                    { title: 'Beskrivelse', type: 'text', property: 'Name', placeholder: 'Prosjektbeskrivelse' }
                                ]
                            },
                            {
                                title: 'Prosjektstyring',
                                fields: [
                                    { title: 'Prosjektleder', type: 'async', id: 'ProjectManagerId', url: 'employee', property: 'ProjectManager', resource: 'Employees', placeholder: 'Velg en prosjektleder' },
                                    { title: 'Startdato', type: 'date', property: 'StartDate', placeholder: 'Velg en startdato' }
                                ]
                            }
                        ]
                    },
                    { title: 'Vedlegg', type: 'projectfile' },
                    { title: 'Ordrebekreftelser', type: 'orderconfirmation' },
                    { title: 'Komponenter', type: 'projectcomponent' },
                    { title: 'Dokumentasjon', type: 'documentation' },
                    { title: 'Vedlikehold', type: 'projectmaintenance' },
                    { title: 'Sikkerhet', type: 'projectsafety' },
                    { title: 'Kapitler', type: 'chapter' },
                    { title: 'Erfaringer', type: 'experience' }
                ],
                columns: [
                    { name: 'Prosjekt', property: 'ProjectNumber', type: 'text', width: 12, url: 'Id' },
                    { name: 'Beskrivelse', property: 'Name', type: 'text', width: 30 },
                    { name: 'Kunde', property: 'Customer.Name', type: 'relation', width: 20, link: 'customer', linkProperty: 'Customer', resource: 'Customers', basePermission: true, url: 'CustomerId' },
                    { name: 'Status', type: 'enumeration', enumtype: 'ProjectStatus', width: 8 }
                ]
            },
            permission: {
                from: 'Permissions',
                expand: 'Creator,Editor',
                columns: [
                    { name: 'Navn', property: 'Name', type: 'text', width: 50, readonly: true },
                    { name: 'Info', property: 'Info', type: 'text', width: 50, readonly: true },
                ]
            },
            orderconfirmation: {
                from: 'OrderConfirmations',
                expand: 'Creator,Editor,Project,Vendor,CustomerReference',
                tabs: [
                    {
                        title: 'Egenskaper',
                        sections: [
                            {
                                title: 'Ordredetaljer',
                                fields: [
                                    { title: 'Ordrenummer', type: 'text', property: 'OrderNumber', placeholder: 'Velg et ordrenummer' },
                                    { title: 'OrderDate', type: 'date', property: 'OrderDate', placeholder: 'Velg en ordredato' },
                                    { title: 'Leverandør', type: 'async', id: 'VendorId', url: 'vendor', property: 'Vendor', resource: 'Vendors', placeholder: 'Velg en leverandør' },
                                    { title: 'Ref. leverandør', type: 'text', property: 'VendorReference', placeholder: 'Velg en leverandørreferanse' },
                                ]
                            },
                            {
                                title: 'Kundedetaljer',
                                fields: [
                                    { title: 'Kundenummer', type: 'text', property: 'CustomerNumber', placeholder: 'Velg et kundenummer' },
                                    { title: 'Kundereferanse', type: 'async', id: 'CustomerReferenceId', url: 'employee', property: 'CustomerReference', resource: 'Employees', placeholder: 'Velg en kundereferanse' },
                                    { title: 'Fakturaadresse', type: 'textarea', property: 'InvoiceAddress', placeholder: 'Velg en fakturaadresse' },
                                    { title: 'Leveringsadresse', type: 'textarea', property: 'DeliveryAddress', placeholder: 'Velg en leveringsadresse' },
                                ]
                            },
                            {
                                title: 'Betalingsdetaljer',
                                fields: [
                                    { title: 'Betalingsmetode', type: 'text', property: 'PaymentMethod', placeholder: 'Velg en betalingsmetode' },
                                    { title: 'Betalingsbetingelser', type: 'text', property: 'PaymentConditions', placeholder: 'Velg betalingsbetingelser' },
                                ]
                            },
                            {
                                title: 'Leveringsdetaljer',
                                fields: [
                                    { title: 'Leveringsmetode', type: 'text', property: 'DeliveryMethod', placeholder: 'Velg en leveringsmetode' },
                                    { title: 'Leveringsbetingelser', type: 'text', property: 'DeliveryConditions', placeholder: 'Velg leveringsbetingelser' },
                                    { title: 'Merket', type: 'text', property: 'Tag', placeholder: 'Merket' },
                                ]
                            },
                            {
                                title: 'Sum',
                                fields: [
                                    { title: 'Totalt uten avgifter', type: 'number', property: 'TotalAmountWithoutTaxes' },
                                    { title: 'Totalt med avgifter', type: 'number', property: 'TotalAmountWithTaxes' },
                                ]
                            }
                        ]
                    },
                    { title: 'Linjer', type: 'orderconfirmationitem' }
                ],
                skipAudit: true,
                columns: [
                    { name: 'Navn', property: 'Name', type: 'text', width: 40, url: 'Id' },
                    { name: 'Beskrivelse', property: 'Description', type: 'text', width: 40 },
                    { name: 'Leverandør', property: 'Vendor.Name', type: 'relation', resource: 'Vendors', width: 20, link: 'vendor', linkProperty: 'Vendor', basePermission: true, url: 'VendorId' }
                ]
            },
            orderconfirmationitem: {
                skipAudit: true,
                columns: [
                    { name: 'Artikkel', property: 'Component.Name', type: 'text', width: 10, url: 'ComponentId', readonly: true },
                    { name: 'Beskrivelse', property: 'Component.Description', type: 'text', width: 20, readonly: true },
                    { name: 'Detaljer', property: 'OrderSpecificDescription', type: 'text', width: 20 },
                    { name: 'Kvantum', property: 'Quantity', type: 'number', width: 10 },
                    { name: 'Enhet', property: 'Unit', type: 'enumeration', enumtype: 'Units', width: 10 },
                    { name: 'Enhetspris', property: 'AmountPerUnit', type: 'number', width: 10 },
                    { name: 'Rabatt', property: 'Discount', type: 'number', width: 10 },
                    { name: 'Totalt', property: 'TotalAmount', type: 'number', width: 10 }
                ]
            },
            experience: {
                from: 'Experiences',
                expand: 'Creator,Editor,Project,Component',
                tabs: [
                    { title: 'Egenskaper', template: '/app/partials/experience/properties.html' }
                ],
                columns: [
                    { name: 'Navn', property: 'Name', type: 'text', width: 36, url: 'Id' },
                    { name: 'Tagger', property: 'Tags', type: 'text', width: 36 }
                ]
            },
            file: {
                from: 'Files',
                expand: 'Creator,Editor,Folder,Folder.Parent,Folder.Parent.Parent,Folder.Parent.Parent.Parent,Folder.Parent.Parent.Parent.Parent,Folder.Parent.Parent.Parent.Parent.Parent',
                tabs: [
                    { title: 'Prosjekter', type: 'projectfile' },
                    { title: 'Komponenter', type: 'componentfile' },
                    { title: 'Dokumentasjon', type: 'documentation' },
                    { title: 'Vedlikehold', type: 'maintenance' },
                    { title: 'Sikkerhet', type: 'safety' },
                    { title: 'Kapitler', type: 'chapterfile' }
                ]
            },
            folder: {
                columns: [
                    { name: 'Filnavn', property: 'Name', type: 'text', width: 36, url: 'Id' },
                    { name: 'Type', property: 'Type', type: 'text', readonly: true, width: 8 },
                    { name: 'Beskrivelse', property: 'Description', type: 'text', width: 36 },
                    { name: 'Størrelse', property: 'FileSize', type: 'number', width: 10, readonly: true, formatter: 'getFormattedSize' },
                    { name: 'Overskrivbar', property: 'IsOverwritable', type: 'bool', width: 10 }
                ]
            },
            employee: {
                from: 'Employees',
                expand: 'Account,ManagedProjects,Tasks',
                tabs: [
                    {
                        title: 'Egenskaper',
                        sections: [
                            {
                                title: 'Generelt',
                                fields: [
                                    { title: 'Navn', type: 'text', property: 'Name', placeholder: 'Navn på ansatt' },
                                    { title: 'Tittel', type: 'text', property: 'Title' },
                                    { title: 'Konto', type: 'async', id: 'AccountId', url: 'user', property: 'Account', resource: 'Users', placeholder: 'Velg en brukerkonto' }
                                ]
                            }
                        ]
                    },
                    { title: 'Prosjekter', type: 'project' },
                    { title: 'Oppgaver', type: 'employeetasks' }
                ],
                columns: [
                    { name: 'Navn', property: 'Name', type: 'text', width: 70, url: 'Id' },
                    { name: 'Tittel', property: 'Title', type: 'text', width: 30 }
                ]
            },
            category: {
                from: 'Categories',
                expand: 'Creator,Editor',
                tabs: [
                    { title: 'Komponenter', type: 'component' }
                ],
                columns: [
                    { name: 'Navn', property: 'Name', type: 'text', width: 23, url: 'Id' },
                    { name: 'Beskrivelse', property: 'Description', type: 'text', width: 40 },
                    { name: 'Leverandør', property: 'Vendor.Name', type: 'relation', resource: 'Vendors', width: 15, link: 'vendor', linkProperty: 'Vendor', basePermission: true, url: 'VendorId' },
                    { name: 'Kategori', property: 'Category.Name', type: 'relation', resource: 'Categories', width: 20, link: 'category', linkProperty: 'Category', basePermission: true, url: 'CategoryId' }
                ]
            },
            chapter: {
                from: 'Chapters',
                expand: 'Project,Creator,Editor',
                tabs: [
                    { title: 'Innhold', template: '/app/partials/chapter/content.html' },
                    {
                        title: 'Egenskaper',
                        sections: [
                            {
                                title: 'Generelt',
                                fields: [
                                    { title: 'Navn', type: 'text', property: 'Name', placeholder: 'Velg et kapittelnavn' },
                                    { title: 'Beskrivelse', type: 'text', property: 'Description' },
                                    { title: 'Prosjekt', type: 'async', id: 'ProjectId', url: 'project', property: 'Project', resource: 'Projects', placeholder: 'Velg et prosjekt' }
                                ]
                            }
                        ]
                    },
                    { title: 'Dokumentasjon', type: 'documentationchapter' }
                ],
                columns: [
                    { name: 'Navn', property: 'Name', type: 'text', width: 40, url: 'Id' },
                    { name: 'Beskrivelse', property: 'Description', type: 'text', width: 40 },
                    { name: 'Prosjekt', property: 'Project.ProjectNumber', type: 'text', width: 30, readonly: true, url: 'ProjectId' }
                ]
            },
            customer: {
                from: 'Customers',
                expand: 'Creator,Editor',
                tabs: [{ title: 'Projects', type: 'project', }],
                columns: [
                    { name: 'Navn', property: 'Name', type: 'text', width: 100, url: 'Id' }
                ]
            },
            abstractcomponent: {
                columns: [
                    { name: 'Name', property: 'Name', type: 'text', width: 23, url: 'Id' },
                    { name: 'Beskrivelse', property: 'Description', type: 'text', width: 40 },
                    { name: 'Leverandør', property: 'Vendor.Name', type: 'relation', resource: 'Vendors', width: 15, link: 'vendor', linkProperty: 'Vendor', basePermission: true, url: 'VendorId' },
                    { name: 'Kategori', property: 'Category.Name', type: 'relation', resource: 'Categories', width: 20, link: 'category', linkProperty: 'Category', basePermission: true, url: 'CategoryId' }
                ]
            },
            component: {
                from: 'Components',
                expand: 'Vendor,Category,Series,Creator,Editor,Image,Image.Folder,Image.Folder.Parent,Image.Folder.Parent.Parent,Image.Folder.Parent.Parent.Parent',
                tabs: [
                    {
                        title: 'Egenskaper',
                        sections: [
                            {
                                title: 'Generelt',
                                fields: [
                                    { title: 'Artikkel', type: 'text', property: 'Name', placeholder: 'Unikt artikkelnummer' },
                                    { title: 'Beskrivelse', type: 'text', property: 'Description' }
                                ]
                            },
                            {
                                title: 'Komponentinformasjon',
                                fields: [
                                    { title: 'Serie', type: 'async', id: 'SeriesId', url: 'componentseries', property: 'Series', resource: 'ComponentSeries', placeholder: 'Velg en komponentserie' },
                                    { title: 'Leverandør', type: 'async', id: 'VendorId', url: 'vendor', property: 'Vendor', resource: 'Vendors', placeholder: 'Velg en leverandør' },
                                    { title: 'Kategori', type: 'async', id: 'CategoryId', url: 'category', property: 'Category', resource: 'Categories', placeholder: 'Velg en kategori' },
                                ]
                            }
                        ]
                    },
                    { title: 'Vedlegg', type: 'componentfile' },
                    { title: 'Vedlikehold', type: 'componentmaintenance' },
                    { title: 'Sikkerhet', type: 'componentsafety' },
                    { title: 'Prosjekter', type: 'projectcomponent' },
                    { title: 'Erfaringer', type: 'experience' }
                ],
                component: {
                    columns: [
                        { name: 'Navn', property: 'Name', type: 'text', width: 23, url: 'Id' },
                        { name: 'Beskrivelse', property: 'Description', type: 'textarea', width: 40 },
                        { name: 'Leverandør', property: 'Vendor.Name', type: 'relation', resource: 'Vendors', width: 15, link: 'vendor', linkProperty: 'Vendor', basePermission: true, url: 'VendorId' },
                        { name: 'Kategori', property: 'Category.Name', type: 'relation', resource: 'Categories', width: 20, link: 'category', linkProperty: 'Category', basePermission: true, url: 'CategoryId' }
                    ]
                },
                vendors: {
                    columns: [
                        { name: 'Navn', property: 'Name', type: 'text', width: 23, url: 'Id' },
                        { name: 'Beskrivelse', property: 'Description', type: 'textarea', width: 40 },
                        { name: 'Leverandør', property: 'Vendor.Name', type: 'relation', resource: 'Vendors', width: 15, link: 'vendor', linkProperty: 'Vendor', basePermission: true, url: 'VendorId' },
                        { name: 'Kategori', property: 'Category.Name', type: 'relation', resource: 'Categories', width: 20, link: 'category', linkProperty: 'Category', basePermission: true, url: 'CategoryId' }
                    ]
                }
            },
            documentation: {
                from: 'Documentations',
                expand: 'Project,Creator,Editor,Image,Image.Folder,Image.Folder.Parent,Image.Folder.Parent.Parent,Image.Folder.Parent.Parent.Parent',
                tabs: [
                    {
                        title: 'Egenskaper',
                        sections: [
                            {
                                title: 'Generelt',
                                fields: [
                                    { title: 'Tittel (norsk)', type: 'text', property: 'Name', placeholder: 'Tittel på dokumentasjon' },
                                    { title: 'Tittel (svensk)', type: 'text', property: 'NameSe', placeholder: 'Tittel på dokumentasjon' },
                                    { title: 'Beskrivelse', type: 'text', property: 'Description' },
                                    { title: 'Prosjekt', type: 'async', id: 'ProjectId', url: 'project', property: 'Project', resource: 'Projects', placeholder: 'Velg et prosjekt' },
                                ]
                            }
                        ]
                    },
                    { title: 'Kapitler', type: 'documentationchapter' }
                ],
                columns: [
                    { name: 'Navn', property: 'Name', type: 'text', width: 70, url: 'Id' },
                    { name: 'Prosjekt', property: 'Project.ProjectNumber', type: 'relation', resource: 'Projects', width: 30, link: 'project', linkProperty: 'Project', url: 'ProjectId' }
                ]
            },
            documentationchapter: {
                documentation: {
                    columns: [
                        { name: '#', property: 'ChapterNumber', type: 'text', width: 5 },
                        { name: 'Navn', property: 'Chapter.Name', type: 'text', width: 20, link: 'chapter', linkProperty: 'Chapter', url: 'ChapterId' },
                        { name: 'Tittel', property: 'Title', type: 'text', width: 15 },
                        { name: 'Beskrivelse', property: 'Chapter.Description', type: 'text', width: 25, readonly: true },
                        { name: 'Ny side', property: 'NewPage', type: 'bool', width: 7 },
                        { name: 'Parametere', property: 'Parameters', type: 'text', width: 18 },
                        { name: 'Prosjekt', property: 'Chapter.Project.ProjectNumber', readonly: true, type: 'text', width: 10, url: 'Chapter.ProjectId' }
                    ]
                },
                chapter: {
                    columns: [
                        { name: 'Navn', property: 'Documentation.Name', type: 'text', width: 70, url: 'Id' },
                        { name: 'Prosjekt', property: 'Documentation.Project.ProjectNumber', type: 'relation', resource: 'Projects', width: 30, link: 'project', linkProperty: 'Documentation.Project', url: 'ProjectId' }
                    ]
                }
            },
            maintenance: {
                from: 'Maintenances',
                expand: 'Vendor,Creator,Editor,Manual,Image.Folder.Parent.Parent,Image.Folder.Parent.Parent.Parent',
                tabs: [
                    {
                        title: 'Egenskaper',
                        sections: [
                            {
                                title: 'Informasjon',
                                fields: [
                                    { title: 'Navn (norsk)', type: 'text', property: 'Name', placeholder: 'Navn på vedlikeholdspunkt' },
                                    { title: 'Navn (svensk)', type: 'text', property: 'NameSe', placeholder: 'Navn på vedlikeholdspunkt' },
                                    { title: 'Beskrivelse (norsk)', type: 'text', property: 'Description', placeholder: 'Beskrivelse' },
                                    { title: 'Beskrivelse (svensk)', type: 'text', property: 'DescriptionSe', placeholder: 'Beskrivelse' },
                                ]
                            },
                            {
                                title: 'Leverandør',
                                fields: [
                                    { title: 'Leverandør', type: 'async', id: 'VendorId', url: 'vendor', property: 'Vendor', resource: 'Vendors', placeholder: 'Velg en leverandør' }
                                ]
                            },
                            {
                                title: 'Manual',
                                fields: [
                                    { title: 'Link til manual', type: 'async', id: 'ManualId', url: 'file', property: 'Manual', resource: 'Files', placeholder: 'Velg en fil' },
                                    { title: 'Sidetall i manual', type: 'number', property: 'ManualPage', placeholder: 'Sidetall' }
                                ]
                            }
                        ]
                    },
                    { title: 'Komponenter', type: 'componentmaintenance' },
                    { title: 'Prosjekter', type: 'projectmaintenance' }
                ],
                columns: [
                    { name: 'Navn', property: 'Name', type: 'text', width: 38, url: 'Id' },
                    { name: 'Leverandør', property: 'Vendor.Name', type: 'relation', resource: 'Vendors', width: 20, link: 'vendor', linkProperty: 'Vendor', basePermission: true, url: 'VendorId' },
                    { name: 'Dag', property: 'IntervalDaily', type: 'bool', width: 5 },
                    { name: 'Uke', property: 'IntervalWeekly', type: 'bool', width: 5 },
                    { name: '2. uke', property: 'IntervalWeekly2', type: 'bool', width: 5 },
                    { name: 'Måned', property: 'IntervalMonthly', type: 'bool', width: 5 },
                    { name: 'Kvartal', property: 'IntervalMonthly3', type: 'bool', width: 5 },
                    { name: 'Halvår', property: 'IntervalHalfYearly', type: 'bool', width: 5 },
                    { name: 'År', property: 'IntervalYearly', type: 'bool', width: 5 },
                    { name: '>År', property: 'IntervalRareky', type: 'bool', width: 5 }
                ]
            },
            componentmaintenance: {
                maintenance: {
                    columns: [
                        { name: 'Leverandør', property: 'Component.Vendor.Name', type: 'text', width: 20, url: 'Component.VendorId', readonly: true },
                        { name: 'Navn', property: 'Component.Name', type: 'text', width: 40, url: 'ComponentId', readonly: true },
                        { name: 'Beskrivelse', property: 'Component.Description', type: 'text', width: 40, readonly: true }
                    ]
                },
                component: {
                    columns: [
                        { name: 'Navn', property: 'Maintenance.Name', type: 'text', width: 25, url: 'MaintenanceId', readonly: true },
                        { name: 'Merknader', property: 'Remarks', type: 'text', width: 30 },
                        { name: 'Dag', property: 'Maintenance.IntervalDaily', type: 'bool', width: 5 },
                        { name: 'Uke', property: 'Maintenance.IntervalWeekly', type: 'bool', width: 5 },
                        { name: '2. uke', property: 'Maintenance.IntervalWeekly2', type: 'bool', width: 5 },
                        { name: 'Måned', property: 'Maintenance.IntervalMonthly', type: 'bool', width: 5 },
                        { name: 'Kvartal', property: 'Maintenance.IntervalMonthly3', type: 'bool', width: 5 },
                        { name: 'Halvår', property: 'Maintenance.IntervalHalfYearly', type: 'bool', width: 5 },
                        { name: 'År', property: 'Maintenance.IntervalYearly', type: 'bool', width: 5 },
                        { name: '>År', property: 'Maintenance.IntervalRarely', type: 'bool', width: 5 },
                    ]
                },
                componentseries: {
                    columns: [
                        { name: 'Navn', property: 'Maintenance.Name', type: 'text', width: 25, url: 'MaintenanceId', readonly: true },
                        { name: 'Merknader', property: 'Remarks', type: 'text', width: 30 },
                        { name: 'Dag', property: 'Maintenance.IntervalDaily', type: 'bool', width: 5 },
                        { name: 'Uke', property: 'Maintenance.IntervalWeekly', type: 'bool', width: 5 },
                        { name: '2. uke', property: 'Maintenance.IntervalWeekly2', type: 'bool', width: 5 },
                        { name: 'Måned', property: 'Maintenance.IntervalMonthly', type: 'bool', width: 5 },
                        { name: 'Kvartal', property: 'Maintenance.IntervalMonthly3', type: 'bool', width: 5 },
                        { name: 'Halvår', property: 'Maintenance.IntervalHalfYearly', type: 'bool', width: 5 },
                        { name: 'År', property: 'Maintenance.IntervalYearly', type: 'bool', width: 5 },
                        { name: '>År', property: 'Maintenance.IntervalRarely', type: 'bool', width: 5 },
                    ]
                }
            },
            componentfile: {
                component: {
                    columns: [
                        { name: 'Fil', property: 'File.Name', type: 'text', width: 40, link: 'file', linkProperty: 'File', readonly: true, url: 'FileId' },
                        { name: 'Rolle', property: 'Role', type: 'enumeration', enumtype: 'FileRoles', width: 10 },
                        { name: 'Leverandør', property: 'Vendor.Name', type: 'relation', resource: 'Vendors', width: 30, link: 'vendor', linkProperty: 'Vendor', basePermission: true, url: 'VendorId' },
                        { name: 'Manual', property: 'IncludeInManual', type: 'bool', width: 10 },
                        { name: 'Utskrift', property: 'IncludedPrintedVersion', type: 'bool', width: 10 }
                    ]
                },
                componentseries: {
                    columns: [
                        { name: 'Fil', property: 'File.Name', type: 'text', width: 40, link: 'file', linkProperty: 'File', readonly: true, url: 'FileId' },
                        { name: 'Rolle', property: 'Role', type: 'enumeration', enumtype: 'FileRoles', width: 10 },
                        { name: 'Leverandør', property: 'Vendor.Name', type: 'relation', resource: 'Vendors', width: 30, link: 'vendor', linkProperty: 'Vendor', basePermission: true, url: 'VendorId' },
                        { name: 'Manual', property: 'IncludeInManual', type: 'bool', width: 10 },
                        { name: 'Utskrift', property: 'IncludedPrintedVersion', type: 'bool', width: 10 }
                    ]
                },
                file: {
                    columns: [
                        { name: 'Leverandør', property: 'Component.Vendor.Name', type: 'text', width: 10, readonly: true, url: 'Component.VendorId' },
                        { name: 'Artikkel', property: 'Component.Name', type: 'text', width: 15, link: 'abstractcomponent', linkProperty: 'Component', url: 'ComponentId', readonly: true },
                        { name: 'Beskrivelse', property: 'Component.Description', type: 'text', width: 25, readonly: true },
                        { name: 'Kategori', property: 'Component.Category.Name', type: 'relation', resource: 'Categries', width: 30, link: 'category', linkProperty: 'Component.Category', basePermission: 'category', url: 'Component.CategoryId' },
                        { name: 'Rolle', property: 'Role', type: 'enumeration', enumtype: 'FileRoles', width: 10 },
                        { name: 'Manual', property: 'IncludeInManual', type: 'bool', width: 10 },
                        { name: 'Utskrift', property: 'IncludedPrintedVersion', type: 'bool', width: 10 }
                    ]
                }
            },
            role: {
                from: 'Roles',
                expand: 'Creator,Editor',
                tabs: [
                    { title: 'Tillatelser', type: 'rolepermission' },
                    { title: 'Brukere', type: 'userrole' }
                ],
                columns: [
                    { name: 'Navn', property: 'Name', type: 'text', width: 100, url: 'Id' }
                ]
            },
            rolepermission: {
                columns: [
                    { name: 'Tillatelse', property: 'Permission.Info', type: 'text', width: 60, readonly: true },
                    { name: 'Les', property: 'Read', type: 'enumeration', enumtype: 'RoleScopes', width: 10 },
                    { name: 'Opprett', property: 'Create', type: 'enumeration', enumtype: 'RoleScopes', width: 10 },
                    { name: 'Endre', property: 'Update', type: 'enumeration', enumtype: 'RoleScopes', width: 10 },
                    { name: 'Slett', property: 'Delete', type: 'enumeration', enumtype: 'RoleScopes', width: 10 }
                ]
            },
            safety: {
                from: 'Safeties',
                expand: 'Creator,Editor,Image,Image.Folder,Image.Folder.Parent,Image.Folder.Parent.Parent,Image.Folder.Parent.Parent.Parent',
                tabs: [
                    {
                        title: 'Egenskaper',
                        sections: [
                            {
                                title: 'Generelt',
                                fields: [
                                    { title: 'Navn (norsk)', type: 'text', property: 'Name', placeholder: 'Navn på sikkerhetspunkt' },
                                    { title: 'Navn (svensk)', type: 'text', property: 'NameSe', placeholder: 'Navn på sikkerhetspunkt' },
                                    { title: 'Beskrivelse', type: 'text', property: 'Description' },
                                    { title: 'Leverandør', type: 'async', id: 'VendorId', url: 'vendor', property: 'Vendor', resource: 'Vendors', placeholder: 'Velg en leverandør' }
                                ]
                            }
                        ]
                    },
                    { title: 'Komponenter', type: 'componentsafety' },
                    { title: 'Prosjekter', type: 'projectsafety' }
                ],
                columns: [
                    { name: 'Navn', property: 'Name', type: 'text', width: 100, url: 'Id' }
                ]
            },
            componentsafety: {
                safety: {
                    columns: [
                        { name: 'Leverandør', property: 'Component.Vendor.Name', type: 'text', width: 20, url: 'Component.VendorId', readonly: true },
                        { name: 'Navn', property: 'Component.Name', type: 'text', width: 40, url: 'ComponentId', readonly: true },
                        { name: 'Beskrivelse', property: 'Component.Description', type: 'text', width: 40, readonly: true }
                    ]
                },
                component: {
                    columns: [
                        { name: 'Navn', property: 'Safety.Name', type: 'text', width: 40, url: 'SafetyId', readonly: true },
                        { name: 'Sted', property: 'Location', type: 'text', width: 40 },
                        { name: 'Rolle', property: 'Role', type: 'enumeration', enumtype: 'SafetyRoles', width: 10 },
                        { name: 'Manual', property: 'IncludeInManual', type: 'bool', width: 10 }
                    ]
                },
                componentseries: {
                    columns: [
                        { name: 'Navn', property: 'Safety.Name', type: 'text', width: 40, url: 'SafetyId', readonly: true },
                        { name: 'Sted', property: 'Location', type: 'text', width: 40 },
                        { name: 'Rolle', property: 'Role', type: 'enumeration', enumtype: 'SafetyRoles', width: 10 },
                        { name: 'Manual', property: 'IncludeInManual', type: 'bool', width: 10 }
                    ]
                }
            },
            componentseries: {
                from: 'ComponentSeries',
                expand: 'Vendor,Category,Creator,Editor',
                tabs: [
                    {
                        title: 'Egenskaper',
                        sections: [
                            {
                                title: 'Generelt',
                                fields: [
                                    { title: 'Serie', type: 'text', property: 'Name', placeholder: 'Navn på komponentserie' },
                                    { title: 'Beskrivelse (norsk)', type: 'text', property: 'Description' },
                                    { title: 'Beskrivelse (svensk)', type: 'text', property: 'DescriptionSe' }
                                ]
                            },
                            {
                                title: 'Komponentserieinformasjon',
                                fields: [
                                    { title: 'Leverandør', type: 'async', id: 'VendorId', url: 'vendor', property: 'Vendor', resource: 'Vendors', placeholder: 'Velg en leverandør' },
                                    { title: 'Kategori', type: 'async', id: 'CategoryId', url: 'category', property: 'Category', resource: 'Categories', placeholder: 'Velg en kategori' },
                                ]
                            }
                        ]
                    },
                    { title: 'Komponenter', type: 'component' },
                    { title: 'Vedlegg', type: 'componentfile' },
                    { title: 'Vedlikehold', type: 'componentmaintenance' },
                    { title: 'Sikkerhet', type: 'componentsafety' },
                    { title: 'Prosjekter', type: 'projectcomponent' }
                ],
                columns: [
                    { name: 'Name', property: 'Name', type: 'text', width: 30, url: 'Id' },
                    { name: 'Beskrivelse', property: 'Description', type: 'text', width: 40 },
                    { name: 'Leverandør', property: 'Vendor.Name', type: 'relation', resource: 'Vendors', width: 30, link: 'vendor', linkProperty: 'Vendor', basePermission: true }
                ]
            },
            user: {
                from: 'Users',
                tabs: [
                    { title: 'Egenskaper', template: '/app/partials/user/properties.html' },
                    { title: 'Roller', type: 'userrole' }
                ],
                columns: [
                    { name: 'Navn', property: 'Name', type: 'text', width: 40, url: 'Id' },
                    { name: 'Brukernavn', property: 'Username', type: 'text', width: 30 },
                    { name: 'Epost', property: 'Email', type: 'text', width: 30 }
                ]
            },
            userrole: {
                role: {
                    columns: [
                        { name: 'Bruker', property: 'User.Name', type: 'text', width: 100, url: 'UserId', readonly: true }
                    ]
                },
                user: {
                    columns: [
                        { name: 'Rolle', property: 'Role.Name', type: 'text', width: 100, url: 'RoleId', readonly: true }
                    ]
                }
            },
            vendor: {
                from: 'Vendors',
                expand: 'Creator,Editor',
                tabs: [
                    {
                        title: 'Egenskaper',
                        sections: [
                            {
                                title: 'Generelt',
                                fields: [
                                    { title: 'Navn', type: 'text', property: 'Name', placeholder: 'Navn på leverandør' },
                                    { title: 'Forkortelse', type: 'text', property: 'ShortName' }
                                ]
                            },
                            {
                                title: 'Kontaktinformasjon',
                                fields: [
                                    { title: 'Postadresse', type: 'textarea', property: 'PostalAddress' },
                                    { title: 'Besøksadresse', type: 'textarea', property: 'VisitingAddress' },
                                    { title: 'Telefonnummer', type: 'text', property: 'PhoneNumber' },
                                    { title: 'Telefaksnummer', type: 'text', property: 'FaxNumber' },
                                    { title: 'Webadresse', type: 'text', property: 'Link' },
                                    { title: 'Epostadresse', type: 'text', property: 'Email' }
                                ]
                            },
                            {
                                title: 'Annet',
                                fields: [
                                    { title: 'Kontonummer', type: 'text', property: 'AccountNumber' },
                                    { title: 'Org. nummer', type: 'text', property: 'OrganizationNumber' }
                                ]
                            }
                        ]
                    },
                    { title: 'Komponenter', type: 'component' }
                ],
                columns: [
                    { name: 'Navn', property: 'Name', type: 'text', width: 70, url: 'Id' },
                    { name: 'Forkortelse', property: 'ShortName', type: 'text', width: 30 }
                ]
            },
        };

        var defaultEntityValues = {
            Category: { Name: 'Ny kategori' },
            Chapter: { Name: 'Nytt kapittel' },
            Component: { Name: 'Ny komponent' },
            ComponentFile: { Name: 'Nytt vedlegg', Role: 'Other', IncludeInManual: true },
            ComponentSeries: { Name: 'Ny komponentserie' },
            ComponentMaintenance: { IncludeInManual: true },
            ComponentSafety: { IncludeInManual: true, Role: 'SafetyRisk' },
            Customer: { Name: 'Ny kunde' },
            Documentation: { Name: 'Ny dokumentasjon' },
            DocumentationChapter: { UniqueKey: utils.generateUid, ChapterNumber: '1' },
            Employee: { Name: 'Ny ansatt' },
            Experience: { Name: 'Ny erfaring' },
            Folder: { Name: 'Ny mappe' },
            Maintenance: { Name: 'Nytt vedlikeholdspunkt' },
            Permission: { Name: 'Ny tillatelse' },
            Project: { ProjectNumber: 'P' + (new Date().getFullYear()) + '-???', Name: 'Nytt prosjekt', Status: 'Planned' },
            ProjectFile: { Name: 'Nytt vedlegg', Role: 'Other', IncludeInManual: true },
            ProjectComponent: { Count: 1, IncludeInManual: true, Unit: 'Units' },
            ProjectMaintenance: { IncludeInManual: true },
            ProjectSafety: { IncludeInManual: true, Role: 'SafetyRisk' },
            Role: { Name: 'Ny rolle' },
            RolePermission: { Read: 'All', Create: 'All', Update: 'All', Delete: 'All' },
            Safety: { Name: 'Nytt sikkerhetspunkt' },
            Vendor: { Name: 'Ny leverandør' },
            User: { Name: 'Ny bruker' }
        };

        var getDefaultValuesForEntity = function(entityType) {

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
                hasChildren: function(item) {
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
                    componentseries: {
                        hierarchy: false,
                        fromId: 'SeriesId',
                        toId: 'Id'
                    },
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
                hasChildren: function(item) {
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
                copy: function(originalEntity) {
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
            getResource: function(route) {
                return _.findWhere(queryParameters, { route: route }).resource;
            }
        };
    });
})();