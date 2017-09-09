function FileUploaderViewModel(options) {
    var self = this;

    self.UploadData = null;
    self.UploadFiles = ko.observableArray([]);

    self.type = 'POST';
	self.datatype = 'JSON';

    self.FilterFiles = false;
    self.Filter = null;

    self.ReadFile = false;
    self.ReaderOnLoad = function(theFile) {
    };

    self.UploadUrl = '';
    self.DropZone = $(options == null || options.DropZone == null ? null : $(options.DropZone));
    self.PasteZone = $(options == null || options.PasteZone == null ? null : $(options.PasteZone));

    self.Add = function(e, data) {
        self.UploadData = data;
        var files = data.files;

        for (var i = 0, f; f = files[i]; i++) {
            // Only process image files.
            if (self.FilterFiles && !f.type.match(self.Filter))
                continue;

            if (self.ReadFile()) {
                var reader = new FileReader();
                reader.onload = (function(theFile) {
                    return self.ReaderOnLoad(theFile);
                })(f);
                reader.readAsDataURL(f);
            }
        }
    };

    self.Submit = function(e, data) {
        data.formData = { data: [] };
        data.files = self.UploadFiles();

        for (var i = 0, f; f = data.files[i]; i++) {
            var igFile = self.GetFile(self.UploadGalleries(), f);
            if (igFile == null)
                continue;

            data.formData.data[i] = {
                EnterpriseId: igFile.ImageGallery.EnterpriseId,
                GalleryName: igFile.ParentGallery.Name,
                IsStockImage: igFile.ImageDetails().IsStockImage(),
                Tags: $.map(igFile.ImageDetails().Tags(), function(item) {
                    return { Id: item.PKID(), Tag: item.Name() }
                }),
                Height: igFile.Height(),
                Width: igFile.Width()
            };
        }

        data.formData.data = JSON.stringify(data.formData.data);
    };

    self.Send = function(e, data) {
    };

    self.Done = function (e, data) {
        self.InsertImages($.map(data.result, function (item) {
            return new ImageGalleryImageViewModel(item, self.SelectedGallery(), self);
        }));
        self.UploadImages.removeAll();
        self.UploadGalleries([new ImageGalleryGalleryViewModel({ Name: ConnectCMS.Globalization.Top, Id: 'root', Type: 'upload' }, null, self)]);
        self.Insert(self.InsertImages());
        self.Close();
    };

    self.Fail = function (e, data) {
    };
    self.Always = function (e, data) {
    };
    self.Progress = function (e, data) {
    };
    self.ProgressAll = function (e, data) {
    };
    self.Start = function (e, data) {
    };
    self.Stop = function (e, data) {
    };
    self.Change = function (e, data) {
    };
    self.Paste = function (e, data) {
    };
    self.Drop = function (e, data) {
    };
    self.DragOver = function (e, data) {
    };
    self.ChuckSend = function (e, data) {
    };
    self.ChunkDone = function (e, data) {
    };
    self.ChunkFail = function (e, data) {
    };
    self.ChunckAlways = function (e, data) {
    };

    self.GetFile = function(galleries, fileData) {
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
}