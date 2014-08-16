
(function () {

    var GanttChartDirective = BaseDirective.extend({
        _notifications: null,
        _element: null,
        _timeout: null,
        _ganttchartElement: null,
        _ganttchartHeaderElement: null,
        _onWindowResizedDebounced: null,


        init: function (scope, element, timeout, notifications) {
            this._element = element;
            this._notifications = notifications;
            this._timeout = timeout;

            this._ganttchartElement = $('#gantt-chart', element);
            this._ganttchartHeaderElement = $('#gantt-header', element);
            this._onWindowResizedDebounced = _.debounce(this._onWindowResized.bind(this), 200).bind(this);

            this._super(scope);
        },

        _postInit: function () {

            var that = this;
            var table = $('.e-table-content', this._element);

            table.scroll(function (e) {
                that._ganttchartElement.scrollTop(e.target.scrollTop);
            });

            this._ganttchartElement.scroll(function (e) {
                table.scrollTop(e.target.scrollTop);
                that._ganttchartHeaderElement.scrollLeft(e.target.scrollLeft);
            });
        },


        defineListeners: function () {
            $(window).on('resize', this._onWindowResizedDebounced);
            this._notifications.addEventListener('performPostInit', this._postInit.bind(this));

            var that = this;
            this._notifications.addEventListener('itemsLoaded', function () {
                that._timeout(that._onWindowResized.bind(that), 0);
            });

            this._notifications.addEventListener('itemsChanged', function () {
                that._timeout(that._onWindowResized.bind(that), 0);
            });

            this._super();
        },

        destroy: function () {
            $(window).off('resize', this._onWindowResizedDebounced);
            // TODO: remove scroll listener on datagrid
            this._ganttchartElement.off();
            this._super();
        },

        _onWindowResized: function () {

            var windowHeight = $(window).height() - $('#grid', this._element).offset().top - $('#statusbar').height() - 64;
            var tableHeight = $('.e-table-content table', this._element).height() - 9;

            this._ganttchartElement.height(windowHeight);
            this._notifications.notify('ganttChartHeightChanged', Math.max(tableHeight, windowHeight - 21));
        }
    });


    angular.module('app').directive('ganttchart', function ($timeout, Notifications) {

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
                new GanttChartDirective($scope, $element, $timeout, Notifications);
            }
        };
    });

})();