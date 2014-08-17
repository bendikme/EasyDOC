var DocumentationController = BaseController.extend({
    _datacontext: null,
    _notifications: null,
    _permissions: null,
    _common: null,

    $location: null,
    $rootScope: null,
    $state: null,

    _queryId: 0,

    init: function ($rootScope, $scope, $state, $location, common, notifications, datacontext, permissions) {

        this._datacontext = datacontext;
        this._notifications = notifications;
        this._permissions = permissions;
        this._common = common;

        this.$location = $location;
        this.$rootScope = $rootScope;
        this.$state = $state;

        this._super($scope);

        if ($state.params.id) {
            this._loadDocumentation($state.params.id);
        }

    },

    defineScope: function () {

        var that = this;

        this._super();
        this.$scope.item = null;

        this.$scope.saveChanges = function () {
            this._datacontext.saveChanges(null, function (error) {
                toastr.error(error.message, 'Noe gikk galt!');
            });
        }.bind(this);

        this.$scope.addChapter = this._addChapter.bind(this);
        this.$scope.toggleSubchapters = this._toggleSubchapters.bind(this);

        this.$scope.hasChanges = this._datacontext.hasChanges;
        this.$scope.cancelChanges = function () {
            that._datacontext.cancelChanges();
            that._prepareChapters(that.$scope.item);
        }

        this.$scope.treeOptions = {
            dropped: function () {
                that._updateChapterNumbers(that.$scope.item, 0);
            }
        };
    },

    defineListeners: function () {
        this._super();
        this.$scope.getAsync = this._getAsync.bind(this);
    },

    _getAsync: function (from, expand, search) {

        if (_.isString(search) && search.length > 0) {

            this.$scope['loading' + from] = true;

            var query = this._datacontext.createQuery()
                .from(from)
                .where('Name', 'Contains', search)
                .orderBy('Name')
                .expand(expand)
                .take(10);

            var that = this;

            return this._datacontext.executeQuery(query, true).finally(function () {
                that.$scope['loading' + from] = false;
            }).then(function (data) {
                return data.results;
            });
        }
    },

    _addChapter: function (rel, chapter) {
        rel.showAddChapter = false;

        var relation = this._datacontext.createEntity('DocumentationChapter', {
            UniqueKey: this._common.generateUid(),
            Documentation: this.$scope.item,
            Chapter: chapter
        });

        this.$scope.item.DocumentationChapters.push(relation);
        rel.collapsed = false;
        rel.subchapters.push(relation);
        this._updateChapterNumbers(this.$scope.item, 0);
        this._prepareChapters(this.$scope.item);
    },

    _toggleSubchapters: function (rel) {
        rel.collapsed = !rel.collapsed;
    },

    _updateChapterNumbers: function (item, level, parent) {
        _.each(item.subchapters, function (rel, index) {
            var chapter = index + 1;
            rel.ChapterNumber = level > 0 ? (parent + '.' + chapter) : chapter;
            this._updateChapterNumbers(rel, level + 1, rel.ChapterNumber);

        }, this);
    },

    _processChapters: function (allChapters, chapter, level) {

        var mainChapter = _.find(allChapters, function (rel) {
            var path = rel.ChapterNumber.split('.');
            if (path.length == level + 1) {
                var number = parseInt(path[level]);
            }
            return number == chapter;
        });

        var subchapters = _.filter(allChapters, function (rel) {
            var path = rel.ChapterNumber.split('.');
            var number = NaN;

            if (path.length > level + 1) {
                number = parseInt(path[level]);
            }

            return number == chapter;
        });

        if (mainChapter) {
            mainChapter.subchapters = [];
            mainChapter.collapsed = true;
        }

        if (subchapters.length) {
            for (var i = 0; i < 10; ++i) {
                var sub = this._processChapters(subchapters, i, level + 1);
                if (sub) {
                    mainChapter.subchapters.push(sub);
                }
            }
        }

        return mainChapter;
    },

    _prepareChapters: function (item) {

        item.subchapters = [];

        for (var i = 0; i < 10; ++i) {
            var sub = this._processChapters(item.DocumentationChapters, i, 0);
            if (sub) {
                item.subchapters.push(sub);
            }
        }
    },

    _loadDocumentation: function (id) {

        var that = this;

        var query = this._datacontext.createQuery()
            .from('Documentations')
            .expand('Creator,Editor,Project,DocumentationChapters.Chapter')
            .where('Id', '==', id);

        this._datacontext.executeQuery(query)
            .then(function (data) {
                var item = data.results[0];
                that.$scope.item = item;
                that._prepareChapters.call(that, item);

            }).catch(function (error) {
                console.log(error);
            });
    }
});

angular.module('app').controller('DocumentationController', ['$rootScope', '$scope', '$state', '$location', 'common', 'Notifications', 'datacontext', 'permissions', DocumentationController]);