
/**
 * Use of Class.js
 */
var DatagridDirective = BaseDirective.extend({
    _notifications: null,
    _element: null,
    _table: null,

    /**
     * @override
     */
    init: function (scope, element, notifications) {
        this._super(scope);
        this._element = element;
        this._notifications = notifications;

        this._table = $('#table', element);
    },


    /**
     * @override
     */
    defineListeners: function () {
        this.$scope.$watch('cursor', this._onCursorUpdated.bind(this));
        $(window).on('resize', this._resizeGrid.bind(this));
    },

    destroy: function () {
        $(window).off('resize', this._resizeGrid.bind(this));
    },

    _onCursorUpdated: function (value) {
        var rowElement = $('#table-body > tbody > tr', this._element)[value];

        if (rowElement) {
            var elmTop = $(rowElement).offset().top;
            var elmHeight = $(rowElement).height();
            var tableTop = this._table.offset().top;
            var tableHeight = this._table.height();
            var scroll = this._table.scrollTop();

            var diff = elmTop + elmHeight - tableTop + scroll;

            if (diff > tableHeight + scroll) {
                this._table.scrollTop(diff - tableHeight);
            } else if (diff - elmHeight < scroll) {
                this._table.scrollTop(diff - elmHeight);
            }
        }
    },

    _resizeGrid: function () {
        this._table.height($(window).height() - this._table.offset().top - $('.statusbar', this._element).outerHeight());
        $('#table-header-wrapper', this._element).width($('#table-body-wrapper', this._element).width());
    }

});


angular.module('datagrid', []).directive('datagrid', function (Notifications) {

    return {
        restrict: 'E',
        templateUrl: '/app/datagrid/datagrid.html',
        transclude: true,
        replace: true,
        controller: 'RemoteGridController',
        scope: {
            ready: '=',
            config: '@',
            items: '='
        },
        link: function ($scope, $element, $attrs) {
            new DatagridDirective($scope, $element, Notifications);
        }
    }
});
