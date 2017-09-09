function MapViewModel(credentials) {
	var self = this;

	self.CenterLatitude = null;
	self.CenterLongitude = null;
	self.CenterRadius = null;
	self.Credentials = credentials;
	self.DisablePanning = false;
	self.DisableZooming = false;
	self.HotelPinClass = null;
	self.HotelPins = ko.observableArray([]);
	self.HoverClass = null;
	self.LocationPinClass = null;
	self.LocationPinDraggable = false;
	self.LocationPins = ko.observableArray([]);
	self.SelectedClass = null;
	self.ShowDashboard = true;

	self.AddHotelPin = function (hotelViewModel) {
		if (hotelViewModel != null)
			self.HotelPins().push(hotelViewModel);
	};
	self.AddLocationPin = function (enterpriseLocationViewModel) {
		if (enterpriseLocationViewModel != null)
			self.LocationPins().push(enterpriseLocationViewModel);
	};
	self.SetCenter = function (latitude, longitude) {
		self.CenterLatitude = latitude;
		self.CenterLongitude = longitude;
	};
	self.SetCenterRadius = function (radius) {
		self.CenterRadius = radius;
	};
};