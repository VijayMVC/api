function RadioButtonViewModel(text, selected, value) {
	var self = this;

	self.Selected = selected;
	self.Value = value;
	self.Text = text;

	self.IsSelected = ko.computed(function () {
		return ko.utils.unwrapObservable(self.Selected) == ko.utils.unwrapObservable(self.Value);
	}, {
		options: {
			deferEvaluation: true
		}
	});
	self.Click = function (data, event) {
		self.Selected(self.Value);
	};
};