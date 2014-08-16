angular.module('app').factory('datacontext', function ($q, settings) {

    breeze.config.initializeAdapterInstance('modelLibrary', 'backingStore', true);
    var manager = new breeze.EntityManager('/breeze/easydoc/');

    man = manager;

    var fetchMetadataPromise = manager.metadataStore.fetchMetadata('/breeze/easydoc/', function () {
        manager.metadataStore.setEntityTypeForResourceName('ComponentSeries', 'ComponentSeries');
        manager.metadataStore.setEntityTypeForResourceName('AllChapters', 'AbstractChapter');
        manager.metadataStore.setEntityTypeForResourceName('AllComponents', 'AbstractComponent');
        manager.metadataStore.setEntityTypeForResourceName('Files', 'File');
        manager.metadataStore.setEntityTypeForResourceName('Folders', 'Folder');
    });

    return new function () {

        var self = this;

        this.whenReady = function (func) {
            fetchMetadataPromise.then(func);
        };

        this.getDeletedEntities = function (type) {

            var singular;
            var ending = type.substr(type.length - 3);
            if (ending == 'ies') {
                singular = type.substr(0, type.length - 3) + 'y';
            } else {
                singular = type.substr(0, type.length - 1);
            }

            try {
                return manager.getEntities(singular, breeze.EntityState.Deleted);
            } catch (e) {
                return manager.getEntities(type, breeze.EntityState.Deleted);
            }
        };

        this.createEntity = function (type, properties) {
            return manager.createEntity(type, properties);
        };

        this.copyEntity = function (type, breezeType, originalEntity) {
            return manager.createEntity(breezeType, settings.copyProperties(type, originalEntity));
        };

        this.createQuery = function () {
            return new breeze.EntityQuery();
        };

        this.executeQueryLocally = function (query) {
            return manager.executeQueryLocally(query);
        };

        this.executeQuery = function (query, fnSuccess, fnError) {
            return manager.executeQuery(query)
			    .then(fnSuccess)
                .catch(fnError);
        };

        this.fetchEntitiesByKey = function (resource, ids, fnSuccess, fnError, expand) {
            var store = manager.metadataStore;
            var type = store.getEntityTypeNameForResourceName(resource);
            var singleKey = store.getEntityType(type).keyProperties[0].name;

            var finalPredicate;
            _.each(ids, function (id) {
                var predicate = breeze.Predicate.create(singleKey, '==', id);
                finalPredicate = finalPredicate ? breeze.Predicate.or([predicate, finalPredicate]) : predicate;
            });

            var query = this.createQuery()
			    .from(resource)
			    .where(finalPredicate)
			    .expand(expand);

            return this.executeQuery(query, fnSuccess, fnError);
        };

        this.getEnumValues = function (type) {

        };

        this.hasChanged = function (fn) {
            manager.hasChangesChanged.subscribe(fn);
        };

        this.hasChanges = function () {
            return manager.hasChanges();
        };

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

        this.cancelChanges = function () {
            manager.rejectChanges();
        };

        this.saveChanges = function (fnSuccess, fnError) {
            return manager.saveChanges()
                .then(fnSuccess)
                .catch(fnError);
        };

        window.onbeforeunload = function () {
            if (self.hasChanges()) {
                return 'You have unsaved changes. Do you really want to continue and lose all your changes?';
            }
        };
    };
});