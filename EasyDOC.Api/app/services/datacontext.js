angular.module('app').factory('datacontext', function ($q, settings) {

    breeze.config.initializeAdapterInstance('modelLibrary', 'backingStore', true);
    var adapter = breeze.config.getAdapterInstance('ajax');
    adapter.defaultSettings = {
        headers: {
            'X-Auth-Token': 'test'
        }
    };

    var manager = new breeze.EntityManager('/breeze/easydoc/');

    man = manager;

    var fetchMetadataPromise = manager.metadataStore.fetchMetadata('/breeze/easydoc/', function () {
        manager.metadataStore.setEntityTypeForResourceName('ComponentSeries', 'ComponentSeries');
        manager.metadataStore.setEntityTypeForResourceName('AllChapters', 'AbstractChapter');
        manager.metadataStore.setEntityTypeForResourceName('AllComponents', 'AbstractComponent');
        manager.metadataStore.setEntityTypeForResourceName('Components', 'Component');
        manager.metadataStore.setEntityTypeForResourceName('Files', 'File');
        manager.metadataStore.setEntityTypeForResourceName('Folders', 'Folder');
    });

    return new function () {

        var self = this;

        this.whenReady = function (func) {
            fetchMetadataPromise.then(func);
        };

        this.createEntity = function (type, properties, state) {
            return manager.createEntity(type, properties, state);
        };

        this.copyEntity = function (type, breezeType, originalEntity) {
            return manager.createEntity(breezeType, settings.copyProperties(type, originalEntity));
        };

        this.createQuery = function () {
            return new breeze.EntityQuery();
        };

        var blockingSemaphore = 0;

        this.executeQuery = function (query, skipBlock) {

            if (!skipBlock) {
                if (blockingSemaphore == 0) {
                    Metronic.blockUI({
                        boxed: true,
                        message: 'Henter data fra server ...'
                    });
                }

                ++blockingSemaphore;
            }

            var promise = manager.executeQuery(query);

            promise.finally(function () {
                if (!skipBlock) {
                    --blockingSemaphore;
                    if (blockingSemaphore <= 0) {
                        Metronic.unblockUI();
                        blockingSemaphore = 0;
                    }
                }
            });

            return promise;
        };

        this.fetchEntitiesByKey = function (resource, ids, fnSuccess, fnError, expand) {
            var store = manager.metadataStore;
            var type = store.getEntityTypeNameForResourceName(resource);
            var singleKey = store.getEntityType(type).keyProperties[0].name;

            var finalPredicate;
            ids.forEach(function (id) {
                var predicate = breeze.Predicate.create(singleKey, '==', id);
                finalPredicate = finalPredicate ? breeze.Predicate.or([predicate, finalPredicate]) : predicate;
            });

            var query = this.createQuery()
					.from(resource)
					.where(finalPredicate)
					.expand(expand);

            return this.executeQuery(query)
				.then(fnSuccess)
				.catch(fnError);
        };

        this.addEntity = manager.addEntity.bind(manager);
        this.attachEntity = manager.attachEntity.bind(manager);
        this.cancelChanges = manager.rejectChanges.bind(manager);
        this.detachEntity = manager.detachEntity.bind(manager);
        this.executeQueryLocally = manager.executeQueryLocally.bind(manager);
        this.getChanges = manager.getChanges.bind(manager);
        this.getEntities = manager.getEntities.bind(manager);
        this.hasChanges = manager.hasChanges.bind(manager);

        this.hasNewItems = function () {
            return _.chain(manager.getChanges())
					.pluck('entityAspect')
					.pluck('entityState')
					.pluck('name')
					.contains('Added')
					.value();
        };

        this.isNewItem = function (item) {
            return item.entityAspect.entityState.name === 'Added';
        };


        this.saveChanges = function (fnSuccess, fnError, fnFinally) {

            Metronic.blockUI({
                boxed: true,
                message: 'Lagrer endringer ...'
            });

            return manager.saveChanges()
			    .then(fnSuccess)
			    .catch(fnError)
			    .finally(function () {
			        Metronic.unblockUI();
			        fnFinally && fnFinally.apply(this, arguments);
			    });
        };

        window.onbeforeunload = function () {
            if (self.hasChanges()) {
                return 'You have unsaved changes. Do you really want to continue and lose all your changes?';
            }
        };
    };
});