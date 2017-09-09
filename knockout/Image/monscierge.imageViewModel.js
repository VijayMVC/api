function ImageViewModel(data) {
	ImageModel.call(this, data);

	var self = this;

	var minimumDetailSize = 130;
	var minimumListItemSize = 65;
	var minimumImageGalleryThumbnailSize = 128;
	var monsciergeImaging = "https://content.monscierge.com/MonsciergeImaging/getImage.ashx?filename=";

	self.DetailImageUrl = ko.computed(function () {
		var result;

		var path = self.Path();

		if (!ConnectCMS.Strings.IsNullOrWhitespace(path))
			result = monsciergeImaging + path + "&width=" + minimumDetailSize + "&height=" + minimumDetailSize + "&mode=zoom";

		return result;
	});
	// TODO: JD: Isn't this computed syntax incorrect?
	// TODO: JD: Isn't 128 close enough to the detail size of 130 we can just use the 130?
	self.ImageGalleryThumbnailImageUrl = ko.computed(function () {
		var result;

		var path = self.Path();

		if (!ConnectCMS.Strings.IsNullOrWhitespace(path))
			if (path.slice(0, 11) === 'data:image/')
				result = self.Path();
			else
				result = monsciergeImaging + path + "&width=" + minimumImageGalleryThumbnailSize + "&height=" + minimumImageGalleryThumbnailSize + "&mode=zoom";

		return result;
	}, {
		deferEvaluation: true
	});
	self.ImageUrl = ko.computed(function () {
		var result;

		var path = self.Path();

		if (!ConnectCMS.Strings.IsNullOrWhitespace(path)) {
		    if (path.indexOf("data:image") == 0) {
		        result = path;
		    }
            else
		        result = monsciergeImaging + self.Path() + '&width=' + self.Width() + '&height=' + self.Height() + '&mode=zoom';
		}

		return result;
	});

    self.CustomImageUrl = function(width, height, mode) {
        return monsciergeImaging + self.Path() + '&width=' + width + '&height=' + height + '&mode=' + mode;
    };

	self.ListItemImageUrl = ko.computed(function () {
		var result;

		var path = self.Path();

		if (!ConnectCMS.Strings.IsNullOrWhitespace(path))
			result = monsciergeImaging + self.Path() + "&width=" + minimumListItemSize + "&height=" + minimumListItemSize + "&mode=zoom";

		return result;
	});

    self.IsLoading = ko.observable(false);
};

ExtendViewModel(ImageViewModel, ImageModel);