(function () {

    angular.module('app').filter('newline', function () {
        return function (text) {
            if (text) return text.replace(/\n/g, '<br/>');
        };
    });

    angular.module('app').directive('ngBindHtmlUnsafe', [function () {
        return function (scope, element, attr) {
            element.addClass('ng-binding').data('$binding', attr.ngBindHtmlUnsafe);
            scope.$watch(attr.ngBindHtmlUnsafe, function ngBindHtmlUnsafeWatchAction(value) {
                element.html(value || '');
            });
        };
    }]);

    angular.module('app').controller('SelectController', function ($scope, datacontext) {

        $scope.loadEnum = function (name) {
            $scope.load(name, true);
        };

        $scope.load = function (entity, isEnum) {

            var ref = $scope[entity.toLowerCase()];
            if (!ref || ref.length == 0) {

                var query = datacontext.createQuery().from(entity);

                if (!isEnum) query = query.orderBy("Name");
                var key = entity.toLowerCase();

                function itemsLoaded(data) {

                    var results = data.results;

                    if (isEnum) {
                        var array = $scope[key] = [];
                        for (var i = 0; i < results.length; ++i) {
                            array.push({ Id: parseInt(i), Name: results[i] });
                        }
                    } else {
                        // Requery from local cache
                        $scope[key] = datacontext.executeQueryLocally(query);
                    }
                };

                function errorLoadingItems(error) {
                    toastr.error('Error fetching records from server.', 'Error!');
                }

                return datacontext.executeQuery(query, itemsLoaded, errorLoadingItems);
            }

            return null;
        };
    });

    angular.module('app').controller('ImportController',
        function ($scope, modal) {

            $scope.import = function () {
                modal.postMessageAndCloseWindow({ content: $scope.importedContent });
            };
        });

    angular.module('app').controller('SearchController',
        function ($scope, $location, datacontext, common) {

            var searchTable = {
                Projects: ['ProjectNumber', 'Name'],
                Customers: ['Name'],
                Vendors: ['Name'],
                Components: ['Name', 'Description'],
                ComponentSeries: ['Name'],
                Employees: ['Name', 'Title'],
                Files: ['Name', 'Type'],
                Experiences: ['Name', 'Tags']
            };

            $scope.searchData = common.search;

            $scope.search = _.debounce(function (term) {

                if (term.length > 0) {

                    $location.path('search');
                    var terms = term.split(' ');

                    for (var resource in searchTable) {

                        var predicates = [];
                        _.each(searchTable[resource], function (property) {
                            var subPredicates = [];
                            _.each(terms, function (currentTerm) {
                                subPredicates.push(breeze.Predicate.create(property, 'Contains', currentTerm));
                            });

                            predicates.push(breeze.Predicate.and(subPredicates));
                        });

                        var predicate = breeze.Predicate.or(predicates);

                        var query = datacontext.createQuery()
                            .from(resource)
                            .where(predicate)
                            .take(20)
                            .inlineCount();

                        function onSuccess(data) {
                            common.search.data[data.query.resourceName] = data.results;
                            common.search.count[data.query.resourceName] = data.inlineCount;
                        }

                        datacontext.executeQuery(query, onSuccess, function (error) {
                            toastr.error(error.Message, "Error!");
                        });
                    }
                }

            }, 500);

        });

    angular.module('app').controller('SaveController',
        function ($scope, datacontext, common) {

            $(window).bind('keydown', function (event) {
                if (event.ctrlKey || event.metaKey) {
                    switch (String.fromCharCode(event.which).toLowerCase()) {
                        case 's':
                            event.preventDefault();

                            if ($scope.hasChanges) {
                                $scope.saveChanges();
                                $scope.$apply();
                            } else {
                                toastr.info("Nothing to save", "Info");
                            }
                            break;

                        case 'r':
                            event.preventDefault();

                            if ($scope.hasChanges) {
                                $scope.cancelChanges();
                                $scope.$apply();
                            } else {
                                toastr.info("No changes to cancel", "Info");
                            }
                    }
                }
            });

            $scope.hasChanges = false;

            $scope.saveChanges = function () {
                common.setBusy(true);
                return datacontext.saveChanges(function (result) {
                    common.setBusy(false);

                    if (result.entities.length == 0) {
                        toastr.error('No items were saved due to validation errors.', 'Error!');
                    } else {
                        toastr.success('All changes have been saved to server.', 'Saved!');
                    }
                }, function (error) {
                    common.setBusy(false);
                    var msg = 'Save failed: ' + breeze.saveErrorMessageService.getErrorMessage(error);
                    toastr.error(msg, 'Error!');
                });
            };

            $scope.cancelChanges = function () {
                datacontext.cancelChanges();
                $scope.$emit('cancelChanges');
                toastr.warning('All changes have been canceled.', 'Canceled!');
            };

            $scope.logout = function () {
                window.location = "/User/Logout";
            };

            datacontext.hasChanged(function (args) {
                var hasChanges = args.hasChanges;
                $scope.hasChanges = hasChanges;
            });
        });
})();