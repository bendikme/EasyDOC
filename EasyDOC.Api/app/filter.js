function FilterCollection(entityProperty) {

	var that = this;
	var filters = [];

	this.apply = function (predicate) {

		_.each(filters, function (filter) {

			var operation = that.getOperators();
			if (_.isNumber(filter.op)) {
				operation = operation[filter.op];
			} else {
				operation = _.findWhere(operation, { op: filter.op });
			}

			var currentPredicate;
			if (entityProperty.type === 'number' || entityProperty.type === 'bool') {
				filter.value = parseInt(filter.value);
				currentPredicate = breeze.Predicate.create(filter.property || entityProperty.property, operation.op || filter.op, filter.value);
			} else {
				currentPredicate = breeze.Predicate.create(filter.property || entityProperty.property, operation.op || filter.op, '\'' + filter.value + '\'');
			}

			if (operation.negate) {
				currentPredicate = breeze.Predicate.not(currentPredicate);
			}

			if (predicate == null) {
				predicate = currentPredicate;
			} else {
				if (filter.bool == FilterCollection.booleanAnd.value) {
					predicate = breeze.Predicate.and([predicate, currentPredicate]);
				} else if (filter.bool == FilterCollection.booleanOr.value) {
					predicate = breeze.Predicate.or([predicate, currentPredicate]);
				}
			}
		});

		return predicate;
	};

	this.add = function (property, bool, op, value) {
		filters.push({
			property: property,
			bool: bool,
			op: op,
			value: value
		});
	}

	this.addDefault = function () {
		if (this.canAdd()) {
			filters.push({
				bool: FilterCollection.booleanAnd.value,
				op: getDefaultOperator(),
				value: getDefaultValue()
			});
		}
	};

	this.setSingleFilter = function (value) {
		if (value) {
			filters = [
			    {
			    	bool: FilterCollection.booleanAnd.value,
			    	op: getDefaultOperator(),
			    	value: value
			    }
			];
		} else {
			this.clear();
		}
	};

	this.getSingleFilter = function () {
		return filters.length > 0 ? filters[0].value : '';
	};

	this.canAdd = function () {
		return filters.length == 0 || lastFilterIsComplete();
	};

	this.clear = function () {
		filters = [];
	};

	this.isEmpty = function () {
		return filters.length == 0;
	};

	this.hasValidFilters = function () {
		return filters.length > 1 || (filters.length == 1 && lastFilterIsComplete());
	};

	this.removeLast = function () {
		filters.pop();
	};

	this.getFilters = function () {
		return filters;
	};

	function getDefaultOperator() {
		return that.getOperators()[0].id;
	};

	function getDefaultValue() {
		return entityProperty.type == 'enumeration' ? entityProperty.enums[0] : '';
	};

	this.getOperators = function () {
		switch (entityProperty.type) {
			case 'text':
			case 'textarea':
			case 'datetime':
				return FilterCollection.operators.string;

			default:
				return FilterCollection.operators[entityProperty.type];
		}
	};


	function lastFilterIsComplete() {
		return filters[filters.length - 1].value;
	}
}

FilterCollection.booleanAnd = { value: 1, name: 'And' };
FilterCollection.booleanOr = { value: 2, name: 'Or' };

FilterCollection.operators = {
	string: [
	    { id: 0, op: 'Contains', negate: false, name: 'Contains' },
	    { id: 1, op: 'Contains', negate: true, name: 'Does not contain' },
	    { id: 2, op: 'EndsWith', negate: false, name: 'Ends with' },
	    { id: 3, op: 'EndsWith', negate: true, name: 'Does not end with' },
	    { id: 4, op: 'Equals', negate: false, name: 'Is equal to' },
	    { id: 5, op: 'NotEquals', negate: false, name: 'Is not equal to' },
	    { id: 6, op: 'StartsWith', negate: false, name: 'Starts with' },
	    { id: 7, op: 'StartsWith', negate: true, name: 'Does not start with' }
	],
	number: [
	    { id: 0, op: 'Equals', negate: false, name: '=' },
	    { id: 1, op: 'NotEquals', negate: false, name: '<>' },
	    { id: 1, op: 'GreaterThan', negate: false, name: '>' },
	    { id: 2, op: 'GreaterThanOrEqual', negate: false, name: '>=' },
	    { id: 3, op: 'LessThan', negate: false, name: '<' },
	    { id: 4, op: 'LessThanOrEqual', negate: false, name: '<=' }
	],
	enumeration: [
	    { id: 0, op: 'Equals', negate: false, name: 'Is equal to' },
	    { id: 1, op: 'Equals', negate: true, name: 'Is not equal to' }
	],
	bool: [
	    { id: 0, op: 'Equals', negate: false, name: 'Is' }
	]
};