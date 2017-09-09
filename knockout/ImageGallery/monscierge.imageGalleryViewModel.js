function ImageGalleryViewModel(options) {
	var self = this;

	options.images = options.images == null ? (options.currentImage != null ? [options.currentImage] : []) : options.images;

	self.ImageContraints = options.imageConstraints;
	self.Selection = options.selection;

	self.PageSizes = ko.observableArray([
		new ImageGalleryPageSizeViewModel(10, self),
		new ImageGalleryPageSizeViewModel(25, self),
		new ImageGalleryPageSizeViewModel(50, self),
		new ImageGalleryPageSizeViewModel(100, self)
	]);

	self.PageSize = ko.observable(self.PageSizes()[0]);

	self.UploadUrl = options != null ? (options.uploadUrl != null ? options.uploadUrl : '/ConnectCMS/ImageGallery/ImageGalleryUploadImages') : '/ConnectCMS/ImageGallery/ImageGalleryUploadImages';
	self.Title = ko.observable(options != null ? (options.title != null ? options.title : ConnectCMS.Globalization.ImageGallery) : ConnectCMS.Gloablization.ImageGallery);
	self.Tabs = ko.observableArray([
		new ImageGalleryTabViewModel({ Name: ConnectCMS.Globalization.Browse, Id: 'browse' }),
		new ImageGalleryTabViewModel({ Name: ConnectCMS.Globalization.Upload, Id: "upload" })
	]);
	self.SelectedImages = ko.observableArray([]);
	self.UploadImages = ko.observableArray([]);
	self.SelectedUploadImage = ko.observable();
	self.AllTags = ko.observableArray([]);
	self.InsertImages = ko.observableArray([]);
	self.SelectedTab = ko.observable(self.Tabs()[0].Id());
	self.ExistingImages = ko.observableArray(options == null ? [] : options.images);
	self.Galleries = ko.observableArray([new ImageGalleryGalleryViewModel({ Name: ConnectCMS.Globalization.Top, Id: 'root', Type: 'browse' }, null, self)]);
	self.selectedGallery = ko.observable(self.Galleries()[0]);
	self.SelectedGallery = ko.computed({
		read: function () {
			if (self.SelectedTab() == 'browse')
				return self.selectedGallery();
			else
				return self.selectedUploadGallery();
		},
		write: function (value) {
			if (self.selectedGallery() != null)
				self.selectedGallery().Clean();

			if (self.SelectedTab() == 'browse') {
				self.selectedGallery(value);

				if (value.GalleryCount() == 0)
					$('.ImageGallery-BrowseItems-Container').css('height', 'calc(100% - 60px)');
				else
					$('.ImageGallery-BrowseItems-Container').css('height', 'calc(100% - 128px - 90px)');
			} else {
				self.selectedUploadGallery(value);
				$('#imageGallery-Upload').fileupload({
					add: function (e, data) {
						self.uploadData = data;
						var files = data.files;

						for (var i = 0, f; f = files[i]; i++) {
							// Only process image files.
							if (!f.type.match('image.*'))
								continue;

							var exists = ko.utils.arrayFirst(self.UploadImages(), function (item) {
								return f.name == item.name && f.size == item.size && f.type == item.type;
							});

							if (exists != null) return;

							self.UploadImages.push(f);
							var reader = new FileReader();

							// Closure to capture the file information.
							reader.onload = (function (theFile) {
								return function (e) {
									var width = 0;
									var height = 0;
									var img = $('<img id="uploadImage" style="display:hidden">'); //Equivalent: $(document.createElement('img'))
									img.attr('src', e.target.result);
									img.appendTo('body');
								    window.setTimeout(function() {
								        width = img.width();
								        height = img.height();
								        img.remove();

								        if (self.ImageContraints != null)
								            if (self.ImageContraints.minWidth != null && width < self.ImageContraints.minWidth && self.ImageContraints.minHeight != null && height < self.ImageContraints.minHeight) {
								                ConnectCMS.LogError({ responseText: 'Uploaded images must be at least ' + self.ImageContraints.minWidth + ' X ' + self.ImageContraints.minHeight + ' pixels' }, 'Image is too small', 'Image is too small');
								                self.UploadImages.remove(theFile);

								                return;
								            } else {
								                if (self.ImageContraints.minWidth != null && width < self.ImageContraints.minWidth) {
								                }
								                if (self.ImageContraints.minHeight != null && height < self.ImageContraints.minHeight) {
								                }
								            }

								        // Render thumbnail.
								        self.SelectedGallery().Images.push(new ImageGalleryImageViewModel({
								            ImageGallery: self.SelectedGallery(),
								            ParentGallery: self.SelectedGallery().ParentGallery,
								            Ordinal: 9999,
								            FKEnterprise: self.EnterpriseId,
								            PKID: null,
								            GUID: null,
								            Name: theFile.name,
								            Path: e.target.result,
								            IsActive: true,
								            Width: width,
								            Height: height,
								            LastModifiedDateTime: theFile.lastModifiedDate
								        }, self.SelectedGallery(), self, theFile));
								    }, 500);
								};
							})(f);

							// Read in the image file as a data URL.
							reader.readAsDataURL(f);
						}
					},
					submit: function (e, data) {
						data.formData = { data: [] };
						data.files = self.UploadImages();

						for (var i = 0, f; f = data.files[i]; i++) {
							var igFile = findFile(self.UploadGalleries(), f);
							if (igFile == null)
								continue;

							data.formData.data[i] = {
								EnterpriseId: igFile.ImageGallery.EnterpriseId,
								GalleryName: igFile.ParentGallery.Name,
								IsStockImage: igFile.ImageDetails().IsStockImage(),
								Tags: $.map(igFile.ImageDetails().Tags(), function (item) {
									return { Id: item.PKID(), Tag: item.Name() }
								}),
								Height: igFile.Height(),
								Width: igFile.Width()
							};
						}

						data.formData.data = JSON.stringify(data.formData.data);
					},
					send: function (e, data) {
					},
					done: function (e, data) {
						self.InsertImages($.map(data.result, function (item) {
							return new ImageGalleryImageViewModel(item, self.SelectedGallery(), self);
						}));
						self.UploadImages.removeAll();
						self.UploadGalleries([new ImageGalleryGalleryViewModel({ Name: ConnectCMS.Globalization.Top, Id: 'root', Type: 'upload' }, null, self)]);
						self.Insert(self.InsertImages());
						self.Close();
					},
					fail: function (e, data) {
					},
					always: function (e, data) {
					},
					progress: function (e, data) {
					},
					progressAll: function (e, data) {
					},
					start: function (e, data) {
					},
					stop: function (e, data) {
					},
					change: function (e, data) {
					},
					paste: function (e, data) {
					},
					drop: function (e, data) {
					},
					dragOver: function (e, data) {
					},
					chunkSend: function (e, data) {
					},
					chunkDone: function (e, data) {
					},
					chunkFail: function (e, data) {
					},
					chunkAlways: function (e, data) {
					},
					url: self.UploadUrl,
					dropZone: $('#imageGallery-UploadItems'),
					pasteZone: $('#imageGallery-UploadItems'),
				});

				if (value.GalleryCount() == 0)
					$('.ImageGallery-UploadItems-Container').css('height', 'calc(100% - 60px)');
				else
					$('.ImageGallery-UploadItems-Container').css('height', 'calc(100% - 128px - 90px)');
			}

			value.Load();
		}
	});

	self.UploadGalleries = ko.observableArray([new ImageGalleryGalleryViewModel({ Name: ConnectCMS.Globalization.Top, Id: 'root', Type: 'upload' }, null, self)]);
	self.selectedUploadGallery = ko.observable(self.UploadGalleries()[0]);

	self.TabSelect = function (tab) {
		self.SelectedTab(tab.Id());

		if (self.SelectedTab() == 'browse')
			self.SelectedGallery(self.Galleries()[0]);
		else
			self.SelectedGallery(self.UploadGalleries()[0]);
	};

	self.InsertSelected = function () {
		self.InsertImages(self.SelectedImages());
		self.Insert(self.InsertImages());
		self.Close();
	};

	self.UploadSelected = function () {
		if (self.uploadData == null)
			return;

		self.uploadData.submit();
	};

	self.uploadData = null;

	self.Up = function () {
		if (self.SelectedGallery().ParentGallery != null)
			self.SelectedGallery(self.SelectedGallery().ParentGallery);
	};

	self.Load = function () {
		ConnectCMS.ShowPleaseWait();
		var load1 = true;
		var load2 = true;
		$.ajax({
			url: '/ConnectCMS/ImageGallery/ImageGalleryLoadGallery',
			data: {
				id: 'root',
				type: 'browse'
			},
			type: 'POST',
			success: function (data) {
				var mappedGalleries = $.map(data.Galleries, function (item) {
					return new ImageGalleryGalleryViewModel(item, self.Galleries()[0], self);
				});
				self.Galleries()[0].GalleryCount(mappedGalleries.length);
				self.Galleries()[0].SubGalleries(mappedGalleries);
				load1 = false;

				if (!load2)
					ConnectCMS.HidePleaseWait();
			},
			error: function (jqXHR, textStatus, errorThrown) {
				load1 = false;

				if (!load2)
					ConnectCMS.HidePleaseWait();

				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			}
		});

		$.ajax({
			url: '/ConnectCMS/ImageGallery/ImageGalleryLoadGallery',
			data: {
				id: 'root',
				type: 'upload'
			},
			type: 'POST',
			success: function (data) {
				var mappedGalleries = $.map(data.Galleries, function (item) {
					return new ImageGalleryGalleryViewModel(item, self.UploadGalleries()[0], self);
				});
				self.UploadGalleries()[0].GalleryCount(mappedGalleries.length);
				self.UploadGalleries()[0].SubGalleries(mappedGalleries);

				if (data.Tags != null)
					self.AllTags($.map(data.Tags, function (item) {
						return new ImageGalleryTagViewModel(item, null, self);
					}));

				load2 = false;

				if (!load1)
					ConnectCMS.HidePleaseWait();
			},
			error: function (jqXHR, textStatus, errorThrown) {
				load2 = false;

				if (!load1)
					ConnectCMS.HidePleaseWait();

				ConnectCMS.LogError(jqXHR, textStatus, errorThrown);
			}
		});
	};

	self.Update = function (data) {
		self.Clean();
		self.PageSize(data.pageSize);
		self.ExistingImages(options == null ? [] : $.map(options.images(), function (item) {
			return new ImageGalleryImageViewModel(item._Data, null, self);
		}));
		self.Load();
	};

	self.Insert = options.insert != null ? options.insert : function () {
	};

	self.Clean = function () {
		self.SelectedTab(self.Tabs()[0].Id());

		self.SelectedImages.removeAll();
		self.InsertImages.removeAll();

		ko.utils.arrayForEach(self.Galleries, function (item) { item.Clean(); });
		self.SelectedGallery(self.Galleries()[0]);
	};

	self.Close = function () {
		self.Clean();
		ConnectCMS.HideImageGallery();
	};

	self.Load();
};