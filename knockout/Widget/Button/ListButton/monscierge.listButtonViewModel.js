function ListButtonViewModel(label, options, settings) {
	var self = this;

	self.Label = ko.observable(label != null ? label : null);
	self.Options = ko.observableArray(options != null ? $.map(options, function (item) {
		return new ListButtonOptionViewModel(item, self);
	}) : null);
	self.OptionsVisible = ko.observable(false);

	self.MinWidth = ko.observable(settings == null || settings.minWidth == null ? 0 : settings.minWidth);

	self.DefaultOption = ko.computed(function () {
		var result;

		var options = self.Options();

		if (!ConnectCMS.IsNullOrEmpty(options))
			result = ko.utils.arrayFirst(options, function (item) {
				return item.Defaultable();
			});

		return result;
	});
	self.DefaultLabel = ko.computed(function () {
		var result;

		if (!ConnectCMS.Strings.IsNullOrWhitespace(self.Label()))
			result = self.Label;
		else {
			var defaultOption = self.DefaultOption();

			if (defaultOption != null)
				result = defaultOption.Label();
			else
				result = ConnectCMS.Globalization.Options;
		}

		return result;
	});
	self.NonDefaultOptions = ko.computed(function () {
		var defaultOption = self.DefaultOption();

		if (defaultOption == null)
			return self.Options();
		else
			return ko.utils.arrayFilter(self.Options(), function (item) {
				return item != defaultOption;
			});
	});

	self.ClickDefault = function () {
		var defaultOption = self.DefaultOption();

		if (defaultOption != null)
			defaultOption.ClickOption();
		else
			self.ClickOptions();
	};
	self.ClickOptions = function () {
		self.OptionsVisible(!self.OptionsVisible());
	};
};