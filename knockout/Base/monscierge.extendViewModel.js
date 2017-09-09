function ExtendViewModel(viewModel, model) {
	viewModel.prototype = Object.create(model.prototype);
	viewModel.prototype.model = function () {
		return new model();
	};
	viewModel.prototype.constructor = viewModel;

	viewModel.prototype.getData = function () {
		return this._Data;
	};
	viewModel.prototype.getJSONData = function (includeChildren) {
		if (includeChildren == null || includeChildren == true)
			return ko.toJSON(this._Data);
		else {
			var newData = ko.toJS(this._Data);

			for (var p1 in newData)
				if ($.isArray(newData[p1]))
					delete newData[p1];

			return ko.toJSON(newData);
		}
	};

	model.prototype.toJSON = function () {
		var copy = ko.toJS(this); //easy way to get a clean copy
		delete copy._Data;

		for (var p1 in this) {
			var exists = false;

			if(this.model != null)
			for (var p2 in this.model())
				if (p1 == p2)
					exists = true;

			if (!exists)
				delete copy[p1];
		}

		return copy; //return the copy to be serialized
	};
};