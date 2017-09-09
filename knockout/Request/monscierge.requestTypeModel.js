var ConnectCMS = ConnectCMS || {};
ConnectCMS.Requests = ConnectCMS.Requests || {};
ConnectCMS.Globalization = ConnectCMS.Globalization || {};

ConnectCMS.Requests.RequestTypeModel = function (data) {
	var self = this;

	self.id = data.Id || 0;
	self.name = data.Name || '';
	self.requestTypeGroupName = data.RequestTypeGroupName || '';
	self.requiresValidation = data.RequiresValidation || false;
	self.estimatedEtaSeconds = data.EstimatedEtaSeconds || 0;
	self.url = data.Url || '';

	self.guestUserName = ko.observable();
	self.guestUserName.extend({ required: true });
	self.guestUserRoomNumber = ko.observable();
	self.guestUserRoomNumber.extend({ required: true });

	self.enableValidation = ko.observable(false);

	self.requestOptions = [];

	if (data.RequestOptions) {
		for (var i = 0; i < data.RequestOptions.length; i++) {
			var item = data.RequestOptions[i];
			var option = new ConnectCMS.Requests.RequestOptionModel(item);
			self.requestOptions.push(option);
		}
	}

	self.requiresValidationText = ko.computed(function () {
		return self.requiresValidation ? ConnectCMS.Globalization.Yes : ConnectCMS.Globalization.No;
	});

	self.estimatedEtaText = ko.computed(function () {
		var seconds = self.estimatedEtaSeconds;

		if (seconds <= 0) {
			return ConnectCMS.Globalization.NoTargetEta;
		} else {
			return ConnectCMS.Globalization.TargetEtaLabel + ': ' + moment.duration(seconds, 's').humanize();
		}
	});

	self.isManagedByConsole = ko.computed(function () {
		return self.url.length > 0;
	});

	self.handledByText = ko.computed(function () {
		return ConnectCMS.Globalization.RequestHandledByGroupLabel + ' ' + self.requestTypeGroupName;
	});

	self.orderedRequestOptions = ko.computed(function () {
		return self.requestOptions.sort(function (left, right) {
			return left.ordinal == right.ordinal ? 0 : (left.ordinal < right.ordinal ? -1 : 1);
		});
	});

	self.specialInstructions = ko.observable('');

	self.isValidForSave = function () {
		var isValid = true;
		self.enableValidation(true);
		for (var k = 0; k < self.orderedRequestOptions().length; k++) {
			var requestOption = self.orderedRequestOptions()[k];
			requestOption.enableValidation(true);
			if (!requestOption.isValid()) {
				isValid = false;
			}
		}
		if (!self.guestUserName.isValid())
			isValid = false;
		if (!self.guestUserRoomNumber.isValid())
			isValid = false;
		return isValid;
	}
};