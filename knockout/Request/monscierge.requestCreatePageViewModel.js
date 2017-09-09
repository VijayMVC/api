var ConnectCMS = ConnectCMS || {};
ConnectCMS.Requests = ConnectCMS.Requests || {};

ConnectCMS.Requests.CreatePageViewModel = function (devices, requestUserId) {
	var self = this;
	var api = ConnectCMS.Requests.RequestApi;

	self.devices = devices;
	self.selectedDevice = ko.observable(self.devices().length == 1 ? self.devices()[0] : null);
	self.requestUserId = requestUserId || 0;
	self.selectedRequestType = ko.observable(null);
	self.savingNewRequest = ko.observable(false);

	self.selectedDevice.subscribe(function(newValue) {
		self.selectedRequestType(null);
	});

	self.init = function() {
		// Load the request types
		ko.utils.arrayForEach(self.devices(), function (device) {
			api.getRequestTypes(device.deviceId, function(json) {
				device.requestTypes.removeAll();
				if (json && json.length > 0) {
					var items = ko.utils.arrayMap(json, function(item) {
						return new ConnectCMS.Requests.RequestTypeModel(item);
					});

					ko.utils.arrayPushAll(device.requestTypes, items);
					device.requestTypes.valueHasMutated();
				}
			}, function(jqXhr, textStatus, errorThrown) {
				var errorId = ErrorLogging.log(textStatus + ' | ' + errorThrown);
				Toast.showGenericError(errorId);
			});
		});

		ko.utils.arrayForEach(self.devices(), function (device) {
			api.getRequestTypesByCategory(device.deviceId, function(json) {
				device.groupedRequestTypes.removeAll();
				if (json && json.length > 0) {
					var items = ko.utils.arrayMap(json, function(item) {
						return {
							name: item.RequestCategory.Name,
							requestTypes: ko.utils.arrayMap(item.RequestCategory.RequestTypes, function(rt) {
								return new ConnectCMS.Requests.RequestTypeModel(rt);
							})
						};
					});

					ko.utils.arrayPushAll(device.groupedRequestTypes, items);
					device.requestTypes.valueHasMutated();
				}
			}, function(jqXhr, textStatus, errorThrown) {
				var errorId = ErrorLogging.log(textStatus + ' | ' + errorThrown);
				Toast.showGenericError(errorId);
			});
		});
	}

	self.saveRequest = function (onSuccessCallback) {
		if (self.selectedRequestType() && self.selectedRequestType().isValidForSave()) {
			var requestTypeId = self.selectedRequestType().id;
			var specialInstructions = self.selectedRequestType().specialInstructions();
			var requestMessage = '';
			var hasInputs = false;

			// Build the request message.
			var newLine = '\r\n';
			var options = self.selectedRequestType().orderedRequestOptions();

			if (options) {
				for (var i = 0; i < options.length; i++) {
					var option = options[i];

					var name = option.name;
					var value = option.optionValue() || '';

					if (option.fieldTypeIsDateTime() && value !== '') {
						// For now, format the datetime into a localized string using moment js.
						// At some point, the values will need to be stored differently so they're
						// available for on-demand localization.
						var optionMoment = moment(value);
						if (optionMoment.isValid()) {
							value = optionMoment.format('lll');
						}
					}

					if (value && String(value).length > 0) {
						requestMessage += (name + ' ' + value + newLine);
						hasInputs = true;
					}
				}
			}

			if (specialInstructions && specialInstructions.length > 0) {
				if (hasInputs) {
					requestMessage += newLine;
				}
				requestMessage += (ConnectCMS.Globalization.RequestSpecialInstructionsLabel + ': ' + specialInstructions + newLine);
			}

			self.savingNewRequest(true);
			api.createRequest(self.requestUserId, requestTypeId, requestMessage, self.selectedRequestType().guestUserName(), self.selectedRequestType().guestUserRoomNumber(), function (result) {
				if (result) {
					if (result > 0) {
						self.cancelNewRequest();
						onSuccessCallback();
					}
				}
				self.savingNewRequest(false);
			}, function (jqXhr, textStatus, errorThrown) {
				var errorId = ErrorLogging.log(textStatus + ' | ' + errorThrown);
				Toast.showGenericError(errorId);
				self.savingNewRequest(false);
			});
		}
	};

	self.cancelNewRequest = function () {
		self.clearNewRequestUserFields();
		self.selectedRequestType(null);
	};

	self.clearNewRequestUserFields = function () {
		if (self.selectedRequestType()) {
			self.selectedRequestType().guestUserName('');
			self.selectedRequestType().guestUserRoomNumber('');
		}

		if (self.selectedDevice() != "") {
			for (var i = 0; i < self.selectedDevice().requestTypes().length; i++) {
				var requestType = self.selectedDevice().requestTypes()[i];
				requestType.specialInstructions('');

				if (requestType.requestOptions) {
					for (var k = 0; k < requestType.requestOptions.length; k++) {
						var option = requestType.requestOptions[k];
						option.optionValue(null);
						option.enableValidation(false);
					}
				}
			}
		}
	};

	self.init();

	return {
		selectedDevice: self.selectedDevice,
		devices: self.devices,
		selectedRequestType: self.selectedRequestType,
		savingNewRequest: self.savingNewRequest,
		saveRequest: self.saveRequest,
		cancelNewRequest: self.cancelNewRequest,
		guestUserName: self.guestUserName,
		guestUserRoomNumber: self.guestUserRoomNumber,
	};
}