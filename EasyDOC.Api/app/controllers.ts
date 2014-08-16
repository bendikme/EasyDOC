(() => {

    var app = angular.module('app');

    app.filter('newline', () => text => text ? text.replace(/\n/g, '<br/>') : text);

    app.directive('ngBindHtmlUnsafe', [() => (scope, element, attr) => {
        element.addClass('ng-binding').data('$binding', attr.ngBindHtmlUnsafe);
        scope.$watch(attr.ngBindHtmlUnsafe, value => {
            element.html(value || '');
        });
    }]);

    app.controller('ImportController',
        ($scope, modal) => {
            $scope.import = () => {
                modal.postMessageAndCloseWindow({ content: $scope.importedContent });
            };
        });

    app.controller('ImageSearchController',
        ($scope, $http) => {

            var component;

            $scope.$on('itemLoaded', (event, data) => {
                component = data[0];
            });

            $scope.search = () => {

                var query = component.Name + ' ' + component.Vendor.Name;

                $scope.searching = true;
                $scope.items = [];

                $http({
                    method: 'GET',
                    url: 'https://www.googleapis.com/customsearch/v1',
                    params: {
                        key: 'AIzaSyBhwAsKgLmZzW18Kbg8eCzxbi5aVi1YCW8',
                        cx: '010316048413040855097:spqgspwtfxs',
                        searchType: 'image',
                        q: query
                    }
                }).success((data) => {
                        $scope.searching = false;
                        $scope.items = data.items;
                    }).error(() => {
                        $scope.searching = false;
                    });
            };

        });

    app.controller('SearchController',
        ($scope, $location, datacontext, common) => {

            var searchTable = {
                Projects: ['ProjectNumber', 'Name'],
                Customers: ['Name'],
                Vendors: ['Name'],
                Components: ['Name', 'Description'],
                ComponentSeries: ['Name'],
                Employees: ['Name', 'Title'],
                Files: ['Name', 'Description', 'Type'],
                Experiences: ['Name', 'Tags']
            };

            function getSearchOrder(resource) {
                if (resource == 'Projects') {
                    return 'ProjectNumber';
                } else {
                    return 'Created';
                }
            }

            $scope.searchData = common.search;

            $scope.search = _.debounce(term => {

                if (term.length > 0) {

                    $location.path('search');
                    var terms = term.split(' ');

                    for (var resource in searchTable) {

                        var predicates = [];
                        _.each(searchTable[resource], (property: String) => {
                            var subPredicates = [];
                            _.each(terms, (currentTerm: Object) => {
                                subPredicates.push(breeze.Predicate.create(property, 'Contains', currentTerm));
                            });

                            predicates.push(breeze.Predicate.and(subPredicates));
                        });

                        var predicate = breeze.Predicate.or(predicates);

                        var query = datacontext.createQuery()
                            .from(resource)
                            .where(predicate)
                            .orderByDesc(getSearchOrder(resource))
                            .take(10)
                            .inlineCount();

                        function onSuccess(data) {
                            common.search.data[data.query.resourceName] = data.results;
                            common.search.count[data.query.resourceName] = data.inlineCount;
                        }

                        datacontext.executeQuery(query, onSuccess, error => {
                            toastr.error(error.Message, "Error!");
                        });
                    }
                }

            }, 500);

        });

    app.controller('SaveController',
        ($scope, datacontext, common) => {

            $(window).bind('keydown', event => {
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

            $scope.saveChanges = () => {
                common.setBusy(true);
                return datacontext.saveChanges(result => {
                    common.setBusy(false);

                    if (result.entities.length == 0) {
                        toastr.error('No items were saved due to validation errors.', 'Error!');
                    } else {
                        toastr.success('All changes have been saved to server.', 'Saved!');
                    }
                }, error => {
                        common.setBusy(false);
                        var msg = 'Save failed: ' + breeze.saveErrorMessageService.getErrorMessage(error);
                        toastr.error(msg, 'Error!');
                    });
            };

            $scope.cancelChanges = () => {
                datacontext.cancelChanges();
                $scope.$emit('cancelChanges');
                toastr.warning('All changes have been canceled.', 'Canceled!');
            };

            $scope.logout = () => {
                window.location.href = "/User/Logout";
            };

            datacontext.hasChanged(args => {
                var hasChanges = args.hasChanges;
                $scope.hasChanges = hasChanges;
            });
        });
})();