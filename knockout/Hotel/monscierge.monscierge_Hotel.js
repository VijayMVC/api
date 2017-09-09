var monscierge_Hotel = function (data, account) {
	var self = this;

	self.Main = account.Main;
	self.Account = account;

	self.PKID = ko.observable(data.PKID);
	self.Name = ko.observable(data.Name);

	self.Device = ko.observable(new monscierge_Device(data.Device, self));
};