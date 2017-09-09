function ImageModel(data) {
	DataModel.call(this, data);

	var self = this;

	self.FKAccount = ko.observable(data != null ? data.FKAccount : null);
	self.GUID = ko.observable(data != null ? data.GUID : null);
	self.Height = ko.observable(data != null ? data.Height : null);
	self.IsActive = ko.observable(data != null ? data.IsActive : null);
	self.LastModifiedDateTime = ko.observable(data != null ? data.LastModifiedDateTime : null);
	self.Name = ko.observable(data != null ? data.Name : null);
	self.Path = ko.observable(data != null ? data.Path : null);
	self.PKID = ko.observable(data != null ? data.PKID : null);
	self.REFOBJID = ko.observable(data != null ? data.REFOBJID : null);
	self.Width = ko.observable(data != null ? data.Width : null);
};

ExtendDataModel(ImageModel);