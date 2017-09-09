function ImageGalleryImageViewModel(data, parentGallery, imageGallery, fileUploadData) {
	ImageViewModel.call(this, data);

	var self = this;

	self.ParentGallery = parentGallery;
	self.ImageGallery = imageGallery;

	self.ImageDetails = ko.observable(new ImageGalleryImageDetailsViewModel(self, self.ImageGallery));
	self.Tags = ko.observableArray(data.Tags != null ? $.map(data.Tags, function (item) {
		return new ImageGalleryTagViewModel(item, self.ParentGallery, self.ImageGallery);
	}) : []);
	self.FileUploadData = fileUploadData;

	self.IsSelected = ko.computed(function () {
		if (self.ImageGallery.SelectedTab() == 'browse')
			return ko.utils.arrayFirst(self.ImageGallery.SelectedImages(), function (item) {
				return (item.PKID() != null && item.PKID() == self.PKID());
			}) != null;
		else
			return self.ImageGallery.SelectedUploadImage() == self;
	}, {
		deferEvaluation: true
	});

	self.IsExisting = ko.computed(function () {
		if (self.ImageGallery.ExistingImages == null) return false;
		return ko.utils.arrayFirst(self.ImageGallery.ExistingImages(), function (item) {
			return (item.PKID() != null && item.PKID() == self.PKID());
		}) != null;
	}, {
		deferEvaluation: true
	});

	self.Select = function () {
		if (self.IsExisting())
			return;

		if (self.ImageGallery.SelectedTab() == 'browse')
			if (self.ImageGallery.Selection != null && self.ImageGallery.Selection == 'multiple')
				if (self.IsSelected())
					self.ImageGallery.SelectedImages.remove(ko.utils.arrayFirst(self.ImageGallery.SelectedImages(), function (item) {
						return (item.PKID() != null && item.PKID() == self.PKID());
					}));
				else
					self.ImageGallery.SelectedImages.push(self);
			else
				if (self.IsSelected())
					self.ImageGallery.SelectedImages.removeAll();
				else {
					self.ImageGallery.SelectedImages.removeAll();
					self.ImageGallery.SelectedImages.push(self);
				}
		else
			if (self.IsSelected())
				self.ImageGallery.SelectedUploadImage(null);
			else
				self.ImageGallery.SelectedUploadImage(self);
	};
};