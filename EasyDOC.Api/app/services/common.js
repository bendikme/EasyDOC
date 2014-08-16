angular.module('app').factory('common', function ($q, datacontext, permissions, settings) {
	return new function () {

		var that = this;
		var isLoaded = false;

		var mostVisited = [];

		function saveMostRecentlyUsedItemsToCache(visited) {

			if (!isLoaded)
				return;

			var toCache = [];
			visited.forEach(function (item) {
				var decodeId = item.id.split(':::');
				toCache.push({
					type: decodeId[0].toLowerCase(),
					id: decodeId[1],
					count: item.count
				});
			});

			localStorage.setItem('recentlyUsedItems', JSON.stringify(toCache));
		};

		function loadMostRecentlyUsedItemsFromCache() {
			var items = JSON.parse(localStorage.getItem('recentlyUsedItems'));
			var grouped = _.groupBy(items, 'type');

			var promises = [];

			_.each(grouped, (function (group, key) {

				var resource = _.findWhere(settings.queryParams, { route: key }).resource;
				var ids = _.pluck(group, 'id');

				promises.push(datacontext.fetchEntitiesByKey(resource, ids, function (data) {
					data.results.forEach(function (entity) {
						that.addToMostVisited(entity);
					});
				}));
			}));

			$q.all(promises).then(function () {
				isLoaded = true;
			});
		}

		permissions.whenReady(function () {
			loadMostRecentlyUsedItemsFromCache();
		});

		this.addToMostVisited = function (item) {

			var id = item.entityAspect._entityKey.entityType.shortName + ":::" + item.Id;
			var visitedItem = _.findWhere(mostVisited, { id: id });

			if (!visitedItem) {
				mostVisited.push({
					id: id,
					entity: item,
					count: 1
				});
			} else {
				visitedItem.count += 1;
			}

			mostVisited.sort(function (e1, e2) {
				return e2.count - e1.count;
			});

			mostVisited = _.first(mostVisited, 50);

			if (_.isFunction(this.mostVisitedCallback)) {
				this.mostVisitedCallback(mostVisited);
			}

			saveMostRecentlyUsedItemsToCache(mostVisited);
		};

		this.search = {
			query: "",
			data: {},
			count: {}
		};

		this.formatters = {

			getFormattedSize: function (filesize) {

				var abbr = ['bytes', 'kB', 'MB', 'GB'];

				var i = 0;
				var size = filesize || 0;
				while (size > 1024) {
					size /= 1024;
					++i;
				}

				return (i == 0 ? size + ' ' : size.toFixed(2)) + abbr[i];
			},

			getUserInitials: function (name) {
				if (name) {
					return name.split(' ').map(function (part) {
						return part[0];
					}).join('');
				}

				return name;
			}
		}

		this.generateUid = function (separator) {
			var delim = separator || '-';

			function s4() {
				return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
			}

			return (s4() + s4() + delim + s4() + delim + s4() + delim + s4() + delim + s4() + s4() + s4());
		};
	};
});