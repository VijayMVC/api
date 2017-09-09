ko.bindingHandlers.imageGalleryOpen = {
	'init': function (element, valueAccessor) {
		var options = valueAccessor();
		var insert = options.insert;
		var images = options.images == null ? (options.image != null ? ko.observableArray([options.image]) : ko.observableArray([])) : options.images;
		var uploadUrl = options.uploadUrl == null ? 'ConnectCMS/ImageGallery/ImageGalleryUploadImages' : options.UploadImages;

		$(element).html(ConnectCMS.Globalization.AddImage);

		$(element).on('click.ImageGallery', function () {
			ConnectCMS.ShowImageGallery(
				{
					enterpriseId: enterpriseId,
					pageSize: pageSize,
					images: images,
					insert: insert,
					uploadUrl: uploadUrl
				});
		});
	},
	'update': function (element, valueAccessor) {
		var options = valueAccessor();
		var insert = options.insert;
		var images = options.images == null ? (options.image != null ? ko.observableArray([options.image]) : ko.observableArray([])) : options.images;
		var uploadUrl = options.uploadUrl == null ? 'ConnectCMS/ImageGallery/ImageGalleryUploadImages' : options.UploadImages;

		if (ConnectCMS.MainViewModel.ImageGalleryViewModel() != null)
			ConnectCMS.MainViewModel.ImageGalleryViewModel().Update({
				enterpriseId: enterpriseId,
				pageSize: pageSize,
				images: images,
				insert: insert,
				uploadUrl: uploadUrl
			});
	}
};

function resizeTagContainer() {
	var tagContainer = $(".ImageGallery-TagAvailable-Container");
	var outer = tagContainer.parents(".ImageGallery-BrowseItems-Tags");
	var first = outer.find('.ImageGallery-TagContainer')[0];
	var header = tagContainer.parent().find('.ImageGallery-Tag-Header');

	$(".ImageGallery-TagAvailable-Container").height(outer[0].clientHeight - first.clientHeight - header[0].clientHeight - 12 - 34);
};

function findFile(galleries, fileData) {
	var img = null;

	for (var i = 0, g; g = galleries[i]; i++) {
		img = ko.utils.arrayFirst(g.Images(), function (item) {
			return fileData.name == item.FileUploadData.name && fileData.size == item.FileUploadData.size && fileData.type == item.FileUploadData.type;
		});

		if (img == null) {
			img = findFile(g.SubGalleries(), fileData);

			if (img != null)
				break;
		} else
			break;
	}

	return img;
};