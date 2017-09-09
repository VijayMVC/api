function ImageFileUploaderViewModel(image, onComplete, contraints, setLoading, options) {
    FileUploaderViewModel.call(options);

    var self = this;

    self.Image = ko.observable();
    self.UploadData = null;
    self.ImageConstraints = contraints != null ? contraints : {};
    self.SetLoading = setLoading;

    self.type = 'POST';
	self.datatype = 'XML';

    self.UploadUrl = "/ConnectCMS/ImageGallery/ImageGalleryUploadImages";

    self.FilterFiles = true;
    self.Filter = 'image.*';

    self.ReadFile = true;
    self.ReaderOnLoad = function (theFile, e, data) {
        var width = 0;
        var height = 0;
        var img = $('<img id="uploadImage" style="display:hidden">'); //Equivalent: $(document.createElement('img'))
        img.attr('src', e.target.result);
        img.appendTo('body');
        window.setTimeout(function () {
            width = img.width();
            height = img.height();
            img.remove();

            if (self.ImageConstraints != null)
                if (self.ImageConstraints.minWidth != null && width < self.ImageConstraints.minWidth && self.ImageConstraints.minHeight != null && height < self.ImageConstraints.minHeight) {
                    ConnectCMS.LogError({ responseText: 'Uploaded images must be at least ' + self.ImageConstraints.minWidth + ' X ' + self.ImageConstraints.minHeight + ' pixels' }, 'Image is too small', 'Image is too small');
                    return;
                } else {
                    if (self.ImageConstraints.minWidth != null && width < self.ImageConstraints.minWidth) {
                        ConnectCMS.LogError({ responseText: 'Uploaded images must be at least ' + self.ImageConstraints.minWidth + ' pixels wide' }, 'Image is too small', 'Image is too small');
                        return;
                    }
                    if (self.ImageConstraints.minHeight != null && height < self.ImageConstraints.minHeight) {
                        ConnectCMS.LogError({ responseText: 'Uploaded images must be at least ' + self.ImageConstraints.minHeight + ' pixels tall' }, 'Image is too small', 'Image is too small');
                        return;
                    }
                }

            // Render thumbnail.
            self.Image(new ImageViewModel({
                Ordinal: 1,
                PKID: null,
                GUID: null,
                Name: theFile.name,
                Path: e.target.result,
                IsActive: true,
                Width: width,
                Height: height,
                LastModifiedDateTime: theFile.lastModifiedDate
            }));

            data.submit();
        }, 500);
    }

    self.Add = function (e, data) {
        self.UploadData = data;
        var files = data.files;

        for (var i = 0, f; f = files[i]; i++) {
            // Only process image files.
            if (self.FilterFiles && !f.type.match(self.Filter))
                continue;

            if (self.ReadFile) {
                var reader = new FileReader();
                reader.onload = (function(theFile) {
                    return function(e) {
                        return self.ReaderOnLoad(theFile, e, data);
                    }
                })(f);
                reader.readAsDataURL(f);
            } else {
                data.submit();
            }
        }
    };

    self.Submit = function (e, data) {
        if (self.SetLoading != null)
            self.SetLoading();

        data.formData = { data: [] };

        if(self.Image() != null)
        data.formData.data[0] = {
            Height: self.Image().Height(),
            Width: self.Image().Width()
        };

        data.formData.data = JSON.stringify(data.formData.data);
    };

    self.Done = onComplete;
}