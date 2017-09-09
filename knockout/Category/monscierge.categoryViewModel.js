function CategoryViewModel(data) {
	CategoryModel.call(this, data);

	var self = this;

	self.DefaultImage(data != null && data.DefaultImage != null ? new ImageViewModel(data.DefaultImage) : null);
	self.Image(data != null && data.Image != null ? new ImageViewModel(data.Image) : null);

	self.FormattedBackgroundColor = ko.computed(function () {
		var result;

		var backgroundColor = self.BackgroundColor();

		if (!ConnectCMS.Strings.IsNullOrWhitespace(backgroundColor)) {
			var formattedBackgroundColor = ConnectCMS.FormatColor(backgroundColor);

			result = formattedBackgroundColor;
		}

		return result;
	});
};

ExtendViewModel(CategoryViewModel, CategoryModel);