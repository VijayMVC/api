function CheckboxViewModel(text, checked, indeterminate) {
	var self = this;

	self.Checked = checked;
	self.Text = text;
	self.Indeterminate = indeterminate;

	self.IsChecked = ko.computed(function () {
		return ko.utils.unwrapObservable(self.Checked);
	}, {
		options: {
			deferEvaluation: true
		}
	});

	self.IsIndeterminate = ko.computed(function () {
		return ko.utils.unwrapObservable(self.Indeterminate);
	}, {
		options: {
			deferEvaluation: true
		}
	});

	self.Click = function (data, event) {
		event.stopPropagation();
		self.Checked(!self.Checked());
	};
};