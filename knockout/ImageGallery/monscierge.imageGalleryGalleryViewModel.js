function ImageGalleryGalleryViewModel(data, parent, imageGallery) {
	var self = this;

	self.ImageGallery = imageGallery;
	self.ParentGallery = parent;
	self.Name = data.Name;
	self.Id = data.Id;
	self.Type = data.Type;
	self.SubGalleries = ko.observableArray([]);
	self.Images = ko.observableArray([]);
	self.Tags = ko.observableArray();
	self.SelectedTags = ko.observableArray([]);
	self.pageSize = ko.observable();
	self.TagFilterText = ko.observable('');

	self.PageSize = ko.computed({
		read: function () {
			if (self.pageSize() == null)
				return self.ImageGallery.PageSize();

			return self.pageSize();
		},
		write: function (value) {
			if (value == 0)
				self.pageSize(self.ImageGallery.PageSizes()[0]);
			else
				self.pageSize(value);

			if (self.RootGallery().Type == 'browse') {
				self.Images.removeAll();

				if (self.Name != ConnectCMS.Globalization.Top)
					self.Load();
			}
		}
	});

	self.page = ko.observable(0);
	self.Page = ko.computed({
		read: function () {
			return self.page();
		},
		write: function (value) {
			self.page(value);
			self.Images.removeAll();

			if (self.Name != ConnectCMS.Globalization.Top)
				self.Load();
		}
	});

	self.FilteredTags = ko.computed(function () {
		return ko.utils.arrayFilter(self.Tags(), function (item) {
			var isntSelected = ko.utils.arrayFirst(self.SelectedTags(), function (tag) {
				return tag.PKID() == item.PKID();
			}) == null;

			var a = self.TagFilterText().toLowerCase();

			var b = item.Name().toLowerCase();
			var isFiltered = a.length < 3 || b.indexOf(a) > -1;

			return isntSelected && isFiltered;
		});
	});

	self.ImageCount = ko.observable(0);
	self.PageCount = ko.computed(function () {
		return Math.ceil(self.ImageCount() / self.PageSize());
	});
	self.GalleryCount = ko.observable(0);

	self.RootGallery = ko.computed(function () {
		var root = self;

		while (root.ParentGallery != null)
			root = root.ParentGallery;

		return root;
	}, {
		deferEvaluation: true
	});

	self.FilteredImages = ko.computed(function () {
		return ko.utils.arrayFilter(self.Images(), function (item) {
			for (var i = 0; i < self.SelectedTags().length; i++) {
				var tag = self.SelectedTags()[i];

				var exists = ko.utils.arrayFirst(item.Tags(), function (itemTag) {
					return tag.PKID() == itemTag.PKID();
				}) != null;

				if (!exists)
					return false;
			}

			return true;
		});
	});

	self.CategoryImageUrl = ko.observable(data.GalleryImageUrl == null ? '' : "http://content.monscierge.com/MonsciergeImaging/getImage.ashx?filename=" + data.GalleryImageUrl + "&width=96&height=96");
	self.Select = function () {
		self.ImageGallery.SelectedGallery(self);
	};

	self.Load = function () {
		self.TagFilterText('');
		if (self.RootGallery().Type == 'browse') {
			ConnectCMS.ShowPleaseWait();
			$.ajax({
				url: '/ConnectCMS/ImageGallery/ImageGalleryLoadGallery',
				data: { id: self.Id, type: self.Type },
				type: 'POST',
				success: function (data) {
					var mappedGalleries = $.map(data.Galleries, function (item) {
						return new ImageGalleryGalleryViewModel(item, self, self.ImageGallery);
					});

					self.SubGalleries(mappedGalleries);

					if (self.SubGalleries().length == 0)
						$('.ImageGallery-BrowseItems-Container').css('height', 'calc(100% - 60px)');
					else
						$('.ImageGallery-BrowseItems-Container').css('height', 'calc(100% - 128px - 90px)');

					if (mappedGalleries.length != self.GalleryCount())
						self.GalleryCount(mappedGalleries.length);

					var imageData = {
						id: self.Id,
						type: self.Type,
						page: self.Page(),
						pageSize: self.PageSize(),
						selectedTags: self.SelectedTags(),
						imageContraints: self.ImageGallery.ImageContraints
					};

					if (self.PageSize() >= 0)
						$.ajax({
							url: '/ConnectCMS/ImageGallery/ImageGalleryLoadGalleryImages',
							contentType: 'application/json',
							dataType: 'json',
							data: ko.toJSON(imageData),
							type: 'POST',
							success: function (result) {
								var mappedImages = $.map(result.Images, function (item) {
									return new ImageGalleryImageViewModel(item, self, self.ImageGallery);
								});

								self.Images(mappedImages);
								self.ImageCount(result.ImageCount);
								self.Tags($.map(result.Tags, function (item) {
									return new ImageGalleryTagViewModel(item, self, self.ImageGallery);
								}));
								self.SelectedTags($.map(result.SelectedTags, function (item) {
									return new ImageGalleryTagViewModel(item, self, self.ImageGallery);
								}));

								resizeTagContainer();

								var ig = $("#imageGallery");
								ig.off("resize.imageGallery.Tag");
								ig.on("resize.imageGallery.Tag", function () {
									resizeTagContainer();
								});

								ConnectCMS.HidePleaseWait();
							},
							error: function (jqXHR, textStatus, errorThrown) {
								ConnectCMS.HidePleaseWait();
								ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
							}
						});
				},
				error: function (jqXHR, textStatus, errorThrown) {
					ConnectCMS.HidePleaseWait();
					ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
				}
			});
		}
	};

	self.AddImages = function () {
		$('#imageGallery-Upload').trigger('click');
	};

	//if(self.Name != 'Top')
	//self.Load();

	self.Clean = function () {
		self.TagFilterText('');
		self.Tags.removeAll();
		self.SelectedTags.removeAll();
		self.Images.removeAll();
		ko.utils.arrayForEach(self.SubGalleries, function (item) { item.Clean(); });
		self.SubGalleries.removeAll();
	};
};