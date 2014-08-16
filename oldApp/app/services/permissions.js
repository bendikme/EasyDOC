var user = null;

angular.module('app').factory('permissions', function ($q, datacontext) {

    return new function () {

        var currentUser = null;
        var permissions = [];
        var that = this;

        var readyPromise = $q.defer();

        datacontext.whenReady(function () {
            that.loadPermissions();
        });

        this.whenReady = function (func) {
            readyPromise.promise.then(func);
        };

        this.loadPermissions = function () {

            var query = datacontext.createQuery()
                .from('GetPermissions');

            datacontext.executeQuery(query, function (data) {
                permissions = data.results[0].permissions;
                currentUser = data.results[0].user;
                readyPromise.resolve();
            }, function (error) {
                toastr.error("Error loading user permissions from server: " + error.message, "Error");
                readyPromise.reject();
            });
        };

        this.hasPermission = function (type, action, id) {

            var permission = _.find(permissions, function (p) {
                return p.Permission.Name.toLowerCase() == type.toLowerCase();
            });

            if (permission == undefined) {
                return false;
            }

            switch (action) {
                case 'View': return permission.Read == 'All' || permission.Read == 'Owned';
                case 'Create': return permission.Create == 'All';
                case 'Delete': return checkRights(id, permission.Delete);
                case 'Modify': return checkRights(id, permission.Update);
                default: return false;
            }
        };

        function checkRights(ownerId, scope) {
            if (scope == 'All') {
                return true;
            }

            if (ownerId != null && currentUser != null) {
                return scope == 'Owned' && ownerId == currentUser.Id;
            }

            return false;
        }
    };
});