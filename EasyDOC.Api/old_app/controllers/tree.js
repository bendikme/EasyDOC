(function () {
    'use strict';

    angular.module('app').controller('tree', function tree($rootScope, $scope, $element, $state, $location, $timeout, datacontext, modal, clipboard, common, settings) {

        var updateTableHeight = _.debounce(function () {
            var top = $("#tree-grid").offset().top;
            var height = $(window).height();
            $("#tree-grid").height(height - top - 64);
        }, 500);

        $(function () {
            updateTableHeight();

            $(window).resize(function () {
                updateTableHeight();
            });
        });

        var toolbar = {
            createItemButton: { order: 0, action: createItem, icon: 'fa-folder' },
            deleteButton: { order: 1, action: deleteItem, icon: 'fa-trash-o' },
            linkButton: { order: 2, action: linkItem, icon: 'fa-link' },
        };

        var id = parseInt($state.params.id);
        var type = $state.current.name;
        var entityInfo = _.findWhere(settings.queryParams, { route: type });

        $scope.toolbar = _.toArray(toolbar);

        $rootScope.$watch(function () {
            updateButtonStates();
        });

        $scope.$on('$stateChangeStart', function (event, toState, toParams, fromState, fromParams) {
            if (datacontext.hasNewItems()) {
                toastr.warning('Save new items before leaving page', 'Warning!');
                $location.path($state.href(fromState, fromParams));
            }
        });

        $scope.toggleChildren = function (item) {
            if (item.entityAspect.entityState.name === 'Unchanged') {
                if (!item.isLoaded) {
                    loadSubitems(item);
                } else {
                    item.isExpanded = !item.isExpanded;
                }
            } else {
                toastr.warning('Save in order to access this item', 'Warning!');
            }
        };

        $scope.selectItem = function (item) {

            extendWithViewModelProps(item);

            if (item.entityAspect.entityState.name === 'Unchanged') {

                if (!item.isLoaded) {
                    loadSubitems(item);
                }

                $scope.selectedItem = item;
                $location.path('/' + entityInfo.route + '/' + item.Id);
            } else {
                toastr.warning('Save in order to access this item', 'Warning!');
            }
        };

        $scope.getSubItems = function (item) {
            return item && item[entityInfo.childrenProperty];
        };

        datacontext.executeQuery(getQuery(id), loadingSuccessRoot, loadingErrorRoot);

        function getQuery(id) {
            return datacontext.createQuery()
                .from(entityInfo.resource)
                .expand(entityInfo.expand)
                .where("Id", "==", id);
        }

        function loadSubitems(item) {
            item.isLoading = true;
            querySubitems(item);
        }

        function querySubitems(parent) {

            var query = getQuery(parent.Id);

            datacontext.executeQuery(query,
                function (data) {
                    parent = data.results[0];
                    parent.isLoading = false;
                    parent.isLoaded = true;
                    parent.hasChildren = parent[entityInfo.childrenProperty].length > 0;
                    parent.isExpanded = parent.hasChildren;
                    extendWithViewModelProps(parent);

                }, function (error) {
                    parent.isLoading = false;
                    toastr.error('Error fetching records from server.', 'Error!');
                });
        }

        function extendWithViewModelProps(parent) {

            var subItems = parent[entityInfo.childrenProperty];
            parent.hasChildren = subItems.length > 0;
            parent.isExpanded = parent.isExpanded && parent.hasChildren;

            for (var i = 0; i < subItems.length; ++i) {
                var subitem = parent[entityInfo.childrenProperty][i];
                subitem.hasChildren = subitem[entityInfo.childrenProperty].length > 0;
                subitem.isExpanded = !!subitem.isExpanded;
                subitem.isLoaded = subitem.isExpanded;
                subitem.isLoading = false;
            }
        }

        var firstRootQuery = true;

        function loadingSuccessRoot(data) {

            var item = data.results[0];
            extendWithViewModelProps(item);

            if (item[entityInfo.parentProperty] == null) {
                $scope.item = item;
            } else {

                if (firstRootQuery) {
                    $scope.selectItem(item);
                    firstRootQuery = false;
                }

                item[entityInfo.parentProperty].isExpanded = true;
                datacontext.executeQuery(getQuery(item[entityInfo.parentPropertyId]), loadingSuccessRoot, loadingErrorRoot);
            }
        }

        function loadingErrorRoot(error) {
            toastr.error('Error fetching records from server.', 'Error!');
        }

        function createItem() {
            var item = datacontext.createEntity(entityInfo.entityType, settings.defaultEntityValues[entityInfo.entityType]);
            $scope.selectedItem[entityInfo.childrenProperty].push(item);
            extendWithViewModelProps($scope.selectedItem);
            $scope.selectedItem.isExpanded = true;
        }


        function deleteItem() {
            if ($scope.selectedItem) {
                $scope.selectedItem.entityAspect.setDeleted();
            }
        }

        var mode = $state.params.mode;
        var isModal = !!mode;

        function linkItem() {
            if (isModal) {
                if (datacontext.hasNewItems()) {
                    toastr.warning('Save item before linking.', 'Warning!');
                    return;
                }
                modal.postMessageAndCloseWindow({ type: type, ids: _.pluck([$scope.selectedItem], 'Id') });
            }
        }

        function updateButtonStates() {

            _.each(toolbar, (function (button) {
                button.visible = true;
            }));

            toolbar.linkButton.disabled = !(mode == 'one' && $scope.selectedItem);
            toolbar.deleteButton.disabled = !$scope.selectedItem || entityInfo.hasChildren($scope.selectedItem);
            toolbar.createItemButton.disabled = !$scope.selectedItem;
        }

        $scope.files = [];

        var sourceItem;
        var canDrop = false;
        var delayedExpandItem;
        var isUploading = false;

        $scope.onDropFiles = function (targetItem, files) {
            if (!isUploading) {
                _.each(files, (function (file) {
                    $scope.files.push(file);
                }));

                $scope.files = _.uniq($scope.files);
                $scope.$apply();
                $scope.uploadFiles(targetItem);
            } else {
                toastr.error('Wait until all file uploads have finished.', 'Error!');
            }
        };

        $scope.onDrop = function (targetItem, transfer) {

            if (targetItem.entityAspect.entityState.name !== 'Unchanged') {
                toastr.warning('Save in order to access this item', 'Warning!');
                return;
            }

            var sourceType = transfer.data[0].type;
            var ids = _.pluck(transfer.data, "id");

            if (sourceType == entityInfo.entityType) {

                datacontext.fetchEntitiesByKey(entityInfo.resource, ids, function (data) {

                    data.results.forEach(function (item) {
                        var previousParent = item[entityInfo.parentProperty];
                        _.defer(function () {
                            extendWithViewModelProps(previousParent);
                        });
                        item[entityInfo.parentProperty] = targetItem;
                        extendWithViewModelProps(item);
                    });

                    extendWithViewModelProps(targetItem);

                }, function (error) {
                    toastr.error("Something went wrong! (" + error.message + ")", "Error!");
                }, entityInfo.expand);

            } else if (sourceType == entityInfo.childType) {

                datacontext.fetchEntitiesByKey(entityInfo.childResource, ids, function (data) {
                    data.results.forEach(function (item) {
                        item[entityInfo.childParent] = targetItem;
                    });
                });
            } else {
                toastr.error("Cannot move this entity here.", "Error!");
            }
        };

        $scope.onDragStart = function (item) {
            sourceItem = item;
        };

        $scope.onDragEnd = function () {
            sourceItem = null;
        };

        $scope.onDragEnter = function (item, event) {

            canDrop = !sourceItem || !isItemChildOfSelf(item, sourceItem.Id);

            if (canDrop) {
                if (item.hasChildren) {
                    delayedExpandItem = $timeout(function () {
                        if (!item.isLoaded) {
                            loadSubitems(item);
                        } else {
                            item.isExpanded = true;
                        }
                    }, 500);
                }
            }
        };

        $scope.onDragLeave = function (item, event) {
            $timeout.cancel(delayedExpandItem);
        };

        $scope.onDragOver = function (item, event) {
            event.originalEvent.dataTransfer.dropEffect = canDrop ? 'move' : 'none';
        };

        function isItemChildOfSelf(targetItem, sourceItemId) {

            if (targetItem.Id == sourceItemId) {
                return true;
            }

            while (targetItem.Parent != null) {
                targetItem = targetItem.Parent;
                if (targetItem.Id == sourceItemId) {
                    return true;
                }
            }

            return false;
        }

        var progressId = 0;
        $scope.uploadFiles = function (item) {

            $('#uploadFiles').modal('show');
            isUploading = true;

            _.each($scope.files, (function (file) {
                var formData = new FormData();
                formData.append('folderId', item.Id);
                formData.append('file', file);

                var xhr = new XMLHttpRequest();
                xhr.upload.id = progressId;
                xhr.upload.addEventListener('progress', uploadProgress, false);
                xhr.addEventListener('load', uploadComplete, false);

                xhr.open('post', '/breeze/easydoc/upload/', true);
                xhr.send(formData);

                ++progressId;
            }));

            $scope.$apply();
        };

        function uploadProgress(event) {
            $scope.files[this.id].state = "info";
            $scope.files[this.id].progress = Math.round(event.loaded * 100 / event.total);
            $scope.$apply();
        }

        function uploadComplete(event) {
            --progressId;

            switch (this.status) {
                case 300:
                    toastr.error('There is already a file with this name in another folder on the server.', 'Error!');
                    $scope.files[this.upload.id].state = "warning";
                    break;
                case 302:
                    toastr.error('There is already a file with this name in the folder.', 'Error!');
                    $scope.files[this.upload.id].state = "warning";
                    break;
                case 403:
                    $scope.files[this.upload.id].state = "danger";
                    break;
                case 404:
                    toastr.error('Parent folder not found on server. It may have been deleted by another user. Refresh the webpage and try again.', 'Error!');
                    $scope.files[this.upload.id].state = "danger";
                    break;
                case 415:
                    toastr.error('Unsupported upload request.', 'Error!');
                    $scope.files[this.upload.id].state = "danger";
                    break;
                case 500:
                    toastr.error('Error uploading file.', 'Error!');
                    $scope.files[this.upload.id].state = "danger";
                    break;
                default:
                    $scope.files[this.upload.id].state = "success";
                    break;
            }

            if (progressId == 0) {
                isUploading = false;

                if (this.status == 403) {
                    toastr.error('You do not have the required permissions to upload files.', 'Error!');
                } else if (this.status == 200) {
                    toastr.success('All files have been uploaded.', 'Success!');
                }

                $('#uploadFiles').modal('hide');
                $scope.files = [];
                common.load();
            }
        }
    });
})();