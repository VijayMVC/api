function CategoryEditViewModel(categoryViewModel) {
	BaseViewModel.call(this, categoryViewModel, false);

	var self = this;

	self.CategoryViewModel = categoryViewModel;

	self.ImagesDiffer = ko.computed(function () {
		var result = false;

		var defaultImage = self.DefaultImage();
		var image = self.Image();

		if (defaultImage != null && image != null)
			result = defaultImage.Path() != image.Path();

		return result;
	});
};