
/**
 * Use of Class.js
 */
var GanttChartDirective = BaseDirective.extend({
    _notifications: null,
    _element: null,
    _ganttChartElement: null,
    _ganttchartHeaderElement: null,
    _onWindowResizedDebounced: null,


    /**
     * @override
     */
    init: function (scope, element, notifications) {
        this._element = element;
        this._notifications = notifications;

        this._super(scope);

        this._ganttChartElement = $('#gantt-chart', element);
        this._ganttchartHeaderElement = $('#gantt-header', element);
        this._onWindowResizedDebounced = _.debounce(this._onWindowResized.bind(this), 200);
        $(window).on('resize', this._onWindowResizedDebounced.bind(this));
    },

    _postInit: function () {

        var that = this;
        var table = $('#table', this._element);
        var tableHeader = $('#table-header', this._element);

        $('#table-body').resize(function () {
            that.$scope.ganttChartHeight = $('#table-body', this._element).height();
        });

        $('#table-body-wrapper').width(600);
        $('#table-header-wrapper').width(600);

        table.scroll(function (e) {
            that._ganttChartElement.scrollTop(e.target.scrollTop);
            tableHeader.scrollLeft(e.target.scrollLeft);
        });

        this._ganttChartElement.scroll(function (e) {
            table.scrollTop(e.target.scrollTop);
            that._ganttchartHeaderElement.scrollLeft(e.target.scrollLeft);
        });
    },


    /**
     * @override
     */
    defineListeners: function () {
        $(window).on('resize', this._onWindowResizedDebounced);
        this._notifications.addEventListener('performPostInit', this._postInit.bind(this));
    },

    destroy: function () {
        $(window).off('resize', this._onWindowResizedDebounced);
        // TODO: remove scroll listener on datagrid
        this._ganttChartElement.off();
    },

    _onWindowResized: function () {

        var height = $(window).height() - $('#grid', this._element).offset().top - $('#statusbar').height() - 56;
        this._ganttChartElement.height(height);
        this._notifications.notify('ganttChartHeightChanged', $('#table-body', this._element).height());
    }
});


angular.module('app').directive('ganttchart', function (Notifications) {

    return {
        restrict: 'E',
        templateUrl: '/app/templates/gantt.html',
        transclude: true,
        replace: true,
        controller: 'GanttChartController',
        scope: {
            items: '='
        },
        link: function ($scope, $element, $attrs) {
            new GanttChartDirective($scope, $element, Notifications);
        }
    }
});