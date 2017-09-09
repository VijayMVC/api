function ImageViewViewModel(imageViewModel, onClickBack) {
	var self = this;

	self.ImageViewModel = imageViewModel;
	self.OnClickBack = onClickBack;

	self.ErrorVisible = ko.observable(false);

	self.ClickBack = function () {
		self.ErrorVisible(false);

		if (self.OnClickBack != null)
			self.OnClickBack();
	};
};