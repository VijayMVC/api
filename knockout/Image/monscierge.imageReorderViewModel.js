function ImageReorderViewModel(array, onClickBack, onClickUpdate) {
	var self = this;

	self.Array = ko.isObservable(array), array, ko.observableArray(array);
	self.OnClickBack = onClickBack;
	self.OnClickUpdate = onClickUpdate;

	self.ErrorVisible = ko.observable(false);

	self.ClickBack = function () {
		self.ErrorVisible(false);

		if (self.OnClickBack != null)
			self.OnClickBack();
	};
	self.ClickUpdate = function () {
		if (self.OnClickUpdate != null)
			self.OnClickUpdate();
	};
};