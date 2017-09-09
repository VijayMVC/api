function PredictiveItemViewModel(label, value, prefix, predictiveInputViewModel) {
	var self = this;

	self.Label = label;
	self.PredictiveInputViewModel = predictiveInputViewModel;
	self.Prefix = prefix;
	self.Value = value;

	self.ElementId = ko.computed(function () {
		return self.Prefix + "_" + self.Value.toString().replace(" ", "_");
	});

	self.Click = function () {
		var predictiveInputViewModel = self.PredictiveInputViewModel;

		if (predictiveInputViewModel == null)
			return;

		predictiveInputViewModel.Hovered(false);
		predictiveInputViewModel.Text(self.Label);
	};
	self.MouseOver = function (data, event) {
		var result = false;

		var predictiveInputViewModel = self.PredictiveInputViewModel;

		if (predictiveInputViewModel == null)
			return result;

		predictiveInputViewModel.SelectedItem(data);

		return result;
	};
};