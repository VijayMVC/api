function ImageGalleryTagViewModel(data, parentGallery, imageGallery) {
	var self = this;

	self.ParentGallery = parentGallery;
	self.ImageGallery = imageGallery;
	self.Name = ko.observable(data.Name);
	self.PKID = ko.observable(data.PKID);
	self.REFOBJID = data.REFOBJID;
	self.FKAccount = data.FKAccount;
	self.UsageCount = ko.observable(data.UsageCount);

	self.IsSelected = ko.computed({
		read: function () {
			if (self.ParentGallery == null)
				return false;

			return ko.utils.arrayFirst(self.ParentGallery.SelectedTags(), function (item) {
				return item.PKID() == self.PKID();
			}) != null;
		},
		write: function (value) {
			if (self.ParentGallery == null)
				return;

			if (!self.IsSelected() && value) {
				self.ParentGallery.SelectedTags.push(self);
				self.ParentGallery.Tags.remove(self);
			}
			else if (self.IsSelected() && !value) {
				self.ParentGallery.SelectedTags.remove(self);
				self.ParentGallery.Tags.push(self);
			}

			resizeTagContainer();
		},
		deferEvaluation: true
	});
	self.ToggleSelected = function () {
		self.IsSelected(!self.IsSelected());
		self.ParentGallery.Load();
	};
};

ImageGalleryTagViewModel.prototype.toJSON = function () {
	var copy = ko.toJS(this); //easy way to get a clean copy
	delete copy.ParentGallery; //remove an extra property
	delete copy.ImageGallery; //remove an extra property
	delete copy.IsSelected;
	return copy; //return the copy to be serialized
};