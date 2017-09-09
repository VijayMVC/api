function CategoryModel(data) {
	DataModel.call(this, data);

	var self = this;

	self.BackgroundColor = ko.observable(data != null ? data.BackgroundColor : null);
	self.DefaultImage = ko.observable(data != null && data.DefaultImage != null ? new ImageModel(data.DefaultImage) : null);
	self.Image = ko.observable(data != null && data.Image != null ? new ImageModel(data.Image) : null);
	self.Name = ko.observable(data != null ? data.Name : null);
	self.OnDevice = ko.observable(data != null ? data.OnDevice : false);
	self.Order = ko.observable(data != null ? data.Order : null);
	self.PKID = data != null ? data.PKID : null;
};

ExtendDataModel(CategoryModel);