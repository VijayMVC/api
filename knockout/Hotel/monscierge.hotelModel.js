function HotelModel(data) {
	DataModel.call(this, data);

	var self = this;

	self.Location = ko.observable(data != null ? new LocationModel(data.Location) : new LocationModel(null));
	self.Name = ko.observable(data.Name);
	self.PKID = data.PKID;
	self.RadiusInKilometers = ko.observable(data.RadiusInKilometers);
	self.RadiusInMiles = ko.observable(data.RadiusInMiles);
};

ExtendDataModel(HotelModel);