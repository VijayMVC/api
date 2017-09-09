function ListButtonOptionViewModel(data, listButtonViewModel) {
	var self = this;

	self.ListButtonViewModel = listButtonViewModel;

	self.ClickMethod = data.clickMethod;
	self.Defaultable = ko.observable(data.defaultable);
	self.Label = ko.observable(data.label);
	self.AnchorOptions = ko.observable(data.anchorOptions);

	self.ClickOption = function () {
		if (self.ClickMethod != null)
			self.ClickMethod();

		self.ListButtonViewModel.OptionsVisible(false);
	};
};