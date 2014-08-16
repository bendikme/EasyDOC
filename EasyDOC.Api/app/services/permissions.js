var user = null;

angular.module('app').factory('permissions', function ($q, datacontext) {

	return new function () {

		var currentUser = null;
		var permissions = [];
		var that = this;

		var readyPromise = $q.defer();
		var permissionsLoaded = false;

		datacontext.whenReady(function () {
			that.loadPermissions();
		});

		this.lookup = {};

		this.whenReady = function (func) {
			readyPromise.promise.then(func);
			permissionsLoaded = true;
		};

		this.getCurrentUser = function () {
			return currentUser;
		};

		this.getCurrentUserId = function () {
			return currentUser && currentUser.Id;
		};

		this.loadPermissions = function () {

			var query = datacontext.createQuery()
			    .from('GetPermissions');

			datacontext.executeQuery(query)
				.then(function (data) {
					that.lookup.FileRoles = data.results[0].fileRoles;
					that.lookup.ProjectStatus = data.results[0].projectStatus;
					that.lookup.RoleScopes = data.results[0].roleScopes;
					that.lookup.SafetyRoles = data.results[0].safetyRoles;
					that.lookup.TaskConstraints = data.results[0].taskConstraints;
					that.lookup.TaskLinkTypes = data.results[0].taskLinkTypes;
					that.lookup.TaskStates = data.results[0].taskStates;
					that.lookup.Units = data.results[0].units;

					permissions = data.results[0].permissions;
					currentUser = data.results[0].user;
					readyPromise.resolve();
				}).catch(function (error) {
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