function InformationMessageViewModel(label, visibleSelector) {
	var self = this;

	self.Label = label;
	self.VisibleSelector = visibleSelector;

	self.Click = function () {
		if (self.VisibleSelector != null)
			self.VisibleSelector(false);
	};
};