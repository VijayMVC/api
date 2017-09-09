function DataModel(data) {
	var self = this;

	self._Data = data;

	self.HasChildChanged = function () {
		for (var p1 in self._Data) {
			if ($.isArray(self._Data[p1])) {
				var isDataModel = false;

				for (var i = 0; i < self[p1]().length; i++) {
					var child = self[p1]()[i];
					isDataModel = child instanceof DataModel;

					if (!isDataModel)
						break;

					if (child.HasChanged()) {
						return true;
					}
				}
			}
		}

		return false;
	};
	self.HasSelfChanged = function () {
		var result = false;

		for (var p1 in self._Data) {
			if (!$.isArray(self._Data[p1])) {
				if (self[p1] == null)
					continue;

				if ((!ko.isComputed(this[p1]) && ko.isObservable(this[p1]) && this[p1]() != self._Data[p1]) || this[p1] != self._Data[p1])
					result = true;
			} else {
				var child;
				var isDataModel = false;

				for (var i = 0; i < self[p1]().length; i++) {
					child = self[p1]()[i];
					isDataModel = child instanceof DataModel;

					if (!isDataModel)
						break;
				}

				if (!isDataModel)
					if (self[p1]() != self._Data[p1])
						result = true;
			}
		}

		return result;
	};

	self.HasSelfOrChildChanged = function () {
		var result = self.HasSelfChanged();

		if (!result)
			result = self.HasSelfChanged();

		return result;
	};
};