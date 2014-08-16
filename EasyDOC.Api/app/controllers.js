(function () {
	var app = angular.module('app');

	app.filter('newline', function () {
		return function (text) {
			return text ? text.replace(/\n/g, '<br/>') : text;
		};
	});

	app.directive('ngBindHtmlUnsafe', [function () {
		return function (scope, element, attr) {
			element.addClass('ng-binding').data('$binding', attr.ngBindHtmlUnsafe);
			scope.$watch(attr.ngBindHtmlUnsafe, function (value) {
				element.html(value || '');
			});
		};
	}]);

	app.directive('datetimez', function () {
		return {
			restrict: 'A',
			require: 'ngModel',
			link: function (scope, element, attrs, ngModelCtrl) {

				var initialized = false;

				ngModelCtrl.$formatters.unshift(function (modelValue) {
					if (!initialized) initialize();
					return moment(modelValue).format('DD.MM.YYYY');
				});

				function initialize() {

					initialized = true;

					$(element).datepicker({
						weekStart: 1,
						format: 'dd.mm.yyyy'
					}).on('changeDate', function (e) {
						ngModelCtrl.$setViewValue(e.date);
						scope.$apply();
					})//;.datepicker('show');
				}

			}
		};
	});

	app.controller('ImportController', function ($scope, modal) {
		$scope.import = function () {
			modal.postMessageAndCloseWindow({ content: $scope.importedContent });
		};
	});

	app.controller('ImageSearchController', function ($scope, $http) {
		var component;

		$scope.$on('itemLoaded', function (event, data) {
			component = data[0];
		});

		$scope.search = function () {
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
			}).success(function (data) {
				$scope.searching = false;
				$scope.items = data.items;
			}).error(function () {
				$scope.searching = false;
			});
		};
	});

	app.controller('SearchController', function ($scope, $location, datacontext, common) {
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

		$scope.search = _.debounce(function (term) {
			if (term.length > 0) {
				$location.path('search');
				var terms = term.split(' ');

				for (var resource in searchTable) {
					var predicates = [];
					searchTable[resource].forEach(function (property) {
						var subPredicates = [];
						terms.forEach(function (currentTerm) {
							subPredicates.push(breeze.Predicate.create(property, 'Contains', currentTerm));
						});

						predicates.push(breeze.Predicate.and(subPredicates));
					});

					var predicate = breeze.Predicate.or(predicates);

					var query = datacontext.createQuery().from(resource).where(predicate).orderByDesc(getSearchOrder(resource)).take(10).inlineCount();

					function onSuccess(data) {
						common.search.data[data.query.resourceName] = data.results;
						common.search.count[data.query.resourceName] = data.inlineCount;
					}

					datacontext.executeQuery(query)
						.then(onSuccess)
						.catch(function (error) {
							toastr.error(error.Message, "Error!");
						});
				}
			}
		}, 500);
	});

	app.controller('SaveController', function ($scope, datacontext, common) {
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
			return datacontext.saveChanges(function (result) {
				if (result.entities.length == 0) {
					toastr.error('No items were saved due to validation errors.', 'Error!');
				} else {
					toastr.success('All changes have been saved to server.', 'Saved!');
				}
			}, function (error) {
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
			window.location.href = "/User/Logout";
		};

		datacontext.hasChanged(function (args) {
			var hasChanges = args.hasChanges;
			$scope.hasChanges = hasChanges;
		});
	});
})();
//# sourceMappingURL=controllers.js.map
