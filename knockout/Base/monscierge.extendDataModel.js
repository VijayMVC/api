function ExtendDataModel(model) {
	model.prototype = Object.create(DataModel.prototype);
	model.prototype.constructor = model;

	model.prototype.commitData = function (includeChildren) {
		var newData = ko.toJS(this);
		var exists;

		for (var p1 in this._Data) {
			exists = false;

			for (var p2 in this) {
				if (p1 == p2)
					exists = true;
			}

			if (!exists)
				newData[p1] = this._Data[p1];
		}

		for (var p1 in this) {
			exists = false;

			for (var p2 in this._Data) {
				if (p1 == p2)
					exists = true;
			}

			if (!exists)
				delete newData[p1];
		}

		delete newData._Data;

		for (var p1 in newData) {
			if ($.isArray(newData[p1])) {
				for (var i = 0; i < newData[p1].length; i++) {
					var child = newData[p1][i];

					if (child != null && child.CommitData != null) {
						child.CommitData();
						newData[p1][i] = child._Data;
					}
				}
			}
		}

		this._Data = newData;
	};
	model.prototype.revertData = function (includeChildren) {
		var v1;

		for (var p1 in this._Data) {
			var changed = false;

			v1 = this._Data[p1];

			if (!$.isArray(this._Data[p1])) {
				if (this[p1] == null)
					continue;

				if (!ko.isComputed(this[p1]) && ko.isObservable(this[p1]))
					if (this[p1]() instanceof Object && this[p1]().revertData instanceof Function)
						this[p1]().revertData(includeChildren);
					else
						this[p1](v1);
				else
					if (this[p1] instanceof Object && this[p1].revertData instanceof Function)
						this[p1].revertData(includeChildren);
					else
						this[p1] = v1;
			} else if (includeChildren == null || includeChildren == true) {
				var isDataModel = false;
				if (this[p1] == null) continue;
				for (var i = 0; i < this[p1]().length; i++) {
					var child = this[p1]()[i];
					isDataModel = child instanceof DataModel;

					if (!isDataModel)
						break;

					if (child.revertData != null)
						child.revertData(includeChildren);
				}

				if (!isDataModel)
					this[p1](v1);
			}
		}
	};
	model.prototype.cloneData = function() {
		var data = ko.toJS(this);
		for (var p1 in data) {
			if(_.isFunction(data[p1]))
				delete data[p1];
		}
		delete data._Data;
		return data;
	}
};