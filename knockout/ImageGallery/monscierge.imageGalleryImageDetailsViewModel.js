function ImageGalleryImageDetailsViewModel(parentImage, imageGallery) {
	var self = this;

	self.ParentImage = parentImage;
	self.ImageGallery = imageGallery;

	self.IsStockImage = ko.observable(false);
	self.Name = self.ParentImage.Name;
	self.TagFilterText = ko.observable('');
	self.Tags = ko.observableArray([]);

	self.CanAdd = ko.computed(function () {
		if (self.TagFilterText() == '')
			return false;

		return ko.utils.arrayFirst(self.ImageGallery.AllTags(), function (item) {
			var a = self.TagFilterText().toLowerCase();
			var b = item.Name().toLowerCase();

			return b == a;
		}) == null;
	});
	self.AvailableTags = ko.computed(function () {
		return ko.utils.arrayFilter(self.ImageGallery.AllTags(), function (item) {
			var isntSelected = ko.utils.arrayFirst(self.Tags(), function (tag) {
				return tag.PKID() == item.PKID();
			}) == null;

			var a = self.TagFilterText().toLowerCase();

			var b = item.Name().toLowerCase();
			var isFiltered = a.length < 3 || b.indexOf(a) > -1;

			return isntSelected && isFiltered;
		});
	});

	self.Add = function () {
		var vm = new ImageGalleryTagViewModel({
			Name: self.TagFilterText()
		}, ko.utils.arrayFirst(self.ImageGallery.Galleries()[0].SubGalleries(), function (item) {
			return item.Type == 'Stock';
		}), self.ImageGallery);
		self.ImageGallery.AllTags.push(vm);
		self.Tags.push(vm);
		self.TagFilterText('');
	};
	self.ToggleImageTag = function (tag) {
		if (ko.utils.arrayFirst(self.Tags(), function (item) {
			return item.PKID() == tag.PKID();
		}) == null) {
			self.Tags.push(tag);
			self.TagFilterText('');
		} else
			self.Tags.remove(tag);

		self.Tags.sort(function (a, b) {
			return a.Name().localeCompare(b.Name());
		});
	};
};