(function () {

    var DatagridDirective = BaseDirective.extend({
        _notifications: null,
        _element: null,
        _table: null,
        _contentWrapper: null,
        _contentTable: null,
        _headerWrapper: null,
        _headerTable: null,
        _statusBar: null,
        _scrollBarWidth: 0,

        _resizeGridBound: null,

        init: function (scope, element, timeout, notifications) {
            this._resizeGridBound = _.debounce(this._resizeGrid, 100).bind(this);
            this._element = element;
            this._notifications = notifications;

            this._table = $(element);
            this._contentWrapper = $('.e-table-content', this._table);
            this._contentTable = $('table', this._contentWrapper);
            this._headerWrapper = $('.e-table-header', this._table);
            this._headerTable = $('table', this._headerWrapper);
            this._super(scope);

            timeout(this._resizeGrid.bind(this), 0);

            // Create the measurement node
            var scrollDiv = document.createElement("div");
            scrollDiv.className = "scrollbar-measure";
            document.body.appendChild(scrollDiv);

            // Get the scrollbar width
            this._scrollBarWidth = scrollDiv.offsetWidth - scrollDiv.clientWidth;

            // Delete the DIV 
            document.body.removeChild(scrollDiv);
        },


        defineListeners: function () {
            this.$scope.$watch('columns', this._resizeGridBound, true);
            this.$scope.$watch('showFilters', this._resizeGridBound);
            this.$scope.$watch('cursor', this._onCursorUpdated.bind(this));

            $(window).on('resize', this._resizeGridBound);

            $(this._contentWrapper).scroll(function (e) {
                this._headerWrapper.scrollLeft(e.target.scrollLeft);
            }.bind(this));

            this._super();
        },

        destroy: function () {
            $(window).off('resize', this._resizeGridBound);
            this._super();
        },

        _onCursorUpdated: function (value) {
            var rowElement = $('tbody > tr', this._contentWrapper)[value];

            if (rowElement) {
                var elmTop = $(rowElement).offset().top;
                var elmHeight = $(rowElement).height();
                var tableTop = this._contentWrapper.offset().top;
                var tableHeight = this._contentWrapper.height();
                var scroll = this._contentWrapper.scrollTop();

                var diff = elmTop + elmHeight - tableTop + scroll;

                if (diff > tableHeight + scroll) {
                    this._contentWrapper.scrollTop(diff - tableHeight);
                } else if (diff - elmHeight < scroll) {
                    this._contentWrapper.scrollTop(diff - elmHeight);
                }
            }
        },

        _resizeGrid: function () {

            if (!arguments.length) return;
            var statusBar = $('.statusbar', this._table);

            var height = $(window).height() - this._table.offset().top - statusBar.outerHeight() - this._headerWrapper.height() - 20;
            this._table.height(height);

            var realWidth = this._table.width();
            var scrollWidth = Math.max(realWidth, 720) - this._scrollBarWidth;

            this._contentWrapper.height(height);
            this._contentTable.height('auto');

            this._contentWrapper.width(realWidth);
            this._contentTable.width(scrollWidth);

            this._headerWrapper.width(realWidth);
            this._headerTable.width(scrollWidth);

            // Available width to the rest of the columns except for checkbox (20px) and buttons (60px)
            var availableWidth = scrollWidth - 80;

            var relativeWidth = 0;

            _.each(this.$scope.columns, function (column) {
                if (column.visible) {
                    relativeWidth += column.width;
                }
            });

            var ratio = availableWidth / relativeWidth;

            _.each(this.$scope.columns, function (column) {
                if (column.visible) {
                    column.calculatedWidth = ratio * column.width;
                }
            });

            this._updateScope();
        }
    });

    angular.module('datagrid', []).directive('localDatagrid', function ($timeout, Notifications) {

        return {
            restrict: 'E',
            templateUrl: '/app/datagrid/datagrid.html',
            transclude: true,
            replace: true,
            controller: 'LocalGridController',
            scope: {
                items: '='
            },
            link: function ($scope, $element, $attrs) {
                new DatagridDirective($scope, $element, $timeout, Notifications);
            }
        };
    });

    angular.module('datagrid').directive('datagrid', function ($timeout, Notifications) {

        return {
            restrict: 'E',
            templateUrl: '/app/datagrid/datagrid.html',
            transclude: true,
            replace: true,
            controller: 'RemoteGridController',
            scope: {},
            link: function ($scope, $element, $attrs) {
                new DatagridDirective($scope, $element, $timeout, Notifications);
            }
        };
    });
})();