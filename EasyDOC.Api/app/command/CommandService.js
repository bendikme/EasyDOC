
var Command = Class.extend({
    _name: null,
    _execute: null,
    _canExecute: null,
    _undo: null,

    init: function (name, execute, canExecute, undo) {
        this._name = name;
        this._execute = execute;
        this._canExecute = canExecute;
        this._undo = undo;
    },

    execute: function () {
        this._execute.apply();
    },

    canExecute: function () {
        this._canExecute.apply();
    },

    undo: function () {
        this._undo.apply();
    }

});


(function () {

    /**
     * Use of Class.js
     */
    var CommandManager = Class.extend({
        _commands: {},

        /**
     * @override
     */
        init: function () {

        },


        /**
     * @override
     */
        registerCommand: function (command) {
            if (_commands[command.name]) {
                throw command.name + ' already registered!';
            }

            _commands[command.name] = command;
        },

        unregisterCommand: function (command) {
            delete _commands[command.name];
        }

    });


    angular.module('commands', []).factory('commands', function () {
        return new CommandManager();
    });

})();