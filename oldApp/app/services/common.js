angular.module('app').factory('common', function ($q, datacontext, permissions, settings) {
    return new function () {

        var callback;
        var lock = 0;
        var that = this;
        var isLoaded = false;

        var mostVisited = [];

        function saveMostRecentlyUsedItemsToCache(visited) {

            if (!isLoaded)
                return;

            var toCache = [];
            _.each(visited, function (item) {
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

            _.each(grouped, function (group, key) {

                var resource = _.findWhere(settings.queryParams, { route: key }).resource;
                var ids = _.pluck(group, 'id');

                promises.push(datacontext.fetchEntitiesByKey(resource, ids, function (data) {
                    _.each(data.results, function (entity) {
                        that.addToMostVisited(entity);
                    });
                }));
            });

            $q.all(promises).then(function() {
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

        this.setBusyCallback = function (fn) {
            callback = _.debounce(fn, 100);
            callback();
        };

        this.getBusy = function () {
            return lock > 0;
        };

        this.setBusy = function (value) {
            lock += value ? 1 : -1;
            if (lock <= 1 && callback) {
                callback();
            }
        };
    };
});