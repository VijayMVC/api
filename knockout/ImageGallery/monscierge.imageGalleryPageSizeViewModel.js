function ImageGalleryPageSizeViewModel(size, imageGalllery) {
	var self = this;

	self.ImageGallery = imageGalllery;
	self.Size = size;

	self.Label = ko.computed(function () {
		return self.Size;
	});
};