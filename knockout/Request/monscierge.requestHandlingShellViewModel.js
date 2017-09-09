/// <reference path="monscierge.requestHandlingShellViewModel.js" />
/// <reference path="~/Scripts/knockout/Utilities/monscierge.toast.js" />
/// <reference path="~/Scripts/knockout/Request/monscierge.requestListPageViewModel.js" />
/// <reference path="~/Scripts/knockout/Request/monscierge.requestCreatePageViewModel.js" />
/// <reference path="~/Scripts/knockout/Request/monscierge.requestApi.js" />
/// <reference path="monscierge.requestHandlingShellViewModel.js" />
/// <reference path="~/Scripts/knockout-3.2.0.debug.js" />

var ConnectCMS = ConnectCMS || {};
ConnectCMS.Requests = ConnectCMS.Requests || {};
ConnectCMS.Globalization = ConnectCMS.Globalization || {};

ConnectCMS.Requests.ShellViewModel = function (deviceIds, requestUserId, culture) {
	var self = this;
	self.deviceIds = deviceIds;
	
	self.requestUserId = requestUserId;
	self.culture = culture || 'en-US';
	self.selectedPage = ko.observable('list');

	self.devices = ko.observableArray([]);
	ko.utils.arrayForEach(self.deviceIds, function(deviceId) {
		self.devices.push(
			{
				deviceId: deviceId,
				name: ko.observable(""),
				hotelId: ko.observable(0),
				requestTypes: ko.observableArray([]),
				groupedRequestTypes: ko.observableArray([])
			});
	});

	// Sub-models
	self.listPageModel = new ConnectCMS.Requests.ListPageViewModel(self.requestUserId, self.culture);
	self.createPageModel = new ConnectCMS.Requests.CreatePageViewModel(self.devices, self.requestUserId);

	self.showCreatePage = function () {
		self.selectedPage('create');
		self.listPageModel.stopListTimer();
	}

	self.closeCreatePage = function () {
		self.createPageModel.cancelNewRequest();
		self.selectedPage('list');
		self.listPageModel.loadData();
	}

	self.init = function () {
		// Retrieve and set the HotelId
		ko.utils.arrayForEach(self.devices(), function(device) {
			ConnectCMS.Requests.RequestApi.getHotelFromDevice(device.deviceId, function (json) {
				if (json) {
					device.hotelId(json.PKID);
					device.name(json.Name);
				}
			}, function (jqXhr, textStatus, errorThrown) {
				Toast.error(errorThrown, ConnectCMS.Globalization.ToastTitleError);
			});
		});
	}

	self.saveNewRequest = function () {
		self.createPageModel.saveRequest(
			function () {
				// Success
				self.selectedPage('list');
				self.listPageModel.loadData();
				Toast.success(
					ConnectCMS.Globalization.RequestCreatedSuccessMessage,
					ConnectCMS.Globalization.ToastTitleSuccess);
			});
	}

	self.init();

	return {
		selectedPage: self.selectedPage,
		createRequest: self.createRequest,
		listPageModel: self.listPageModel,
		createPageModel: self.createPageModel,
		showCreatePage: self.showCreatePage,
		closeCreatePage: self.closeCreatePage,
		saveNewRequest: self.saveNewRequest
	}
}