
namespace('ui.navigation.events').SPACE = 'ui.navigation.events.SPACE';
namespace('ui.navigation.events').UP = 'ui.navigation.events.UP';
namespace('ui.navigation.events').DOWN = 'ui.navigation.events.DOWN';
namespace('ui.navigation.events').LEFT = 'ui.navigation.events.LEFT';
namespace('ui.navigation.events').RIGHT = 'ui.navigation.events.RIGHT';
namespace('ui.navigation.events').DELETE = 'ui.navigation.events.DELETE';
namespace('ui.navigation.events').NEW = 'ui.navigation.events.NEW';

namespace('ui.navigation').CODES = {
    32: ui.navigation.events.SPACE,
    37: ui.navigation.events.LEFT,
    38: ui.navigation.events.UP,
    39: ui.navigation.events.RIGHT,
    40: ui.navigation.events.DOWN,
    46: ui.navigation.events.DELETE,
    78: [ui.navigation.events.NEW, 'ctrlKey']
}

/**
 * Use of Class.js
 */
var KeyboardDirective = BaseDirective.extend({
    _notifications: null,
    _element: null,

    /**
     * @override
     */
    init: function (scope, element, notifications) {
        this._super(scope);
        this._element = element;
        this._notifications = notifications;
    },


    /**
     * @override
     */
    defineListeners: function () {
        $(window).on('keydown', this._keydown.bind(this));
    },

    destroy: function () {
        $(window).off('keydown', this._keydown.bind(this));
    },

    _keydown: function (event) {

        var evt = ui.navigation.CODES[event.keyCode];

        if (evt) {
            if (_.isArray(evt)) {
                if (event[evt[1]]) {
                    this._notifications.notify(evt, event);
                }
            } else {
                this._notifications.notify(evt, event);
            }
        }
    }
});


angular.module('app').directive('keyboard', function (Notifications) {

    return {
        restrict: 'E',
        link: function ($scope, $element, $attrs) {
            new KeyboardDirective($scope, $element, Notifications);
        }
    }
});
