function TabViewModel(label, visible, enabled, clickMethod, index, selectedIndex) {
	var self = this;

	if (enabled == null)
		enabled = true;

	if (visible == null)
		visible = true;

	self.ClickMethod = clickMethod;
	self.Enabled = ko.isObservable(enabled) ? enabled : ko.observable(enabled);
	self.Index = index;
	self.Label = label;
	self.SelectedIndex = selectedIndex;
	self.Visible = ko.isObservable(visible) ? visible : ko.observable(visible);

	self.IsSelected = ko.computed(function () {
		if (self.SelectedIndex == null)
			return false;
		else
			return self.Index == self.SelectedIndex();
	});

	self.Click = function () {
		var result = false;

		if (self.Enabled()) {
			if (self.SelectedIndex != null)
				self.SelectedIndex(self.Index);

			if (self.ClickMethod != null)
				result = !self.ClickMethod();
			else
				result = true;
		}

		return result;
	};
};