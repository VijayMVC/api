function ImageGalleryTabViewModel(data) {
	var self = this;

	self.Name = ko.observable(data.Name);
	self.Id = ko.observable(data.Id);
};