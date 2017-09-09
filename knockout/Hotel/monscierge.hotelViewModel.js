function HotelViewModel(data, parentViewModel, main) {
	HotelModel.call(this, data);

	var self = this;

	self.Main = main;
	self.ParentViewModel = parentViewModel;

	self.Location = ko.observable(data != null ? new LocationViewModel(data.Location) : new LocationViewModel(null));
};

ExtendViewModel(HotelViewModel, HotelModel);